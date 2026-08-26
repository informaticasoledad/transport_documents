using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Dtd.Infrastructure.Configuration;

/// <summary>
/// Única fuente de verdad del formato de cifrado de secrets: resolución de la master key
/// (<c>ERPAUTH_MASTER_KEY</c>) y descifrado AES-256-GCM de un bloque <c>{Ciphertext, Nonce, Tag}</c>
/// (base64, sin AAD, plaintext UTF-8). La usan <see cref="ErpAuthPostConfigure"/> (client_secret del
/// ERP) y <see cref="DatabaseAuthPostConfigure"/> (contraseña de PostgreSQL), ambos con la
/// **misma master key**.
/// <para>Es la misma convención que la otra app del grupo y que <c>tools/Dtd.Tools.SecretCipher</c>:
/// el ciphertext producido por cualquiera de ellos se descifra aquí tal cual.</para>
/// </summary>
internal static class EncryptedSecretCipher
{
    /// <summary>Resuelve la master key (32 bytes) por orden: config <c>ERPAUTH_MASTER_KEY</c>
    /// (coge user-secrets en Development) → env var <c>ERPAUTH_MASTER_KEY</c> → fichero
    /// <c>ERPAUTH_MASTER_KEY_FILE</c> (montaje de Secret de k8s). Lanza si falta o no es base64/32B.</summary>
    public static byte[] ResolveMasterKey(IConfiguration cfg)
    {
        // 1) Configuration (User Secrets en Development entran aquí)
        var keyB64 = cfg["ERPAUTH_MASTER_KEY"];

        // 2) Env var directa
        keyB64 ??= Environment.GetEnvironmentVariable("ERPAUTH_MASTER_KEY");

        // 3) Fichero (útil en Docker/K8s: montaje de Secret)
        var keyFile = Environment.GetEnvironmentVariable("ERPAUTH_MASTER_KEY_FILE");
        if (string.IsNullOrWhiteSpace(keyB64) && !string.IsNullOrWhiteSpace(keyFile) && File.Exists(keyFile))
            keyB64 = File.ReadAllText(keyFile).Trim();

        if (string.IsNullOrWhiteSpace(keyB64))
            throw new InvalidOperationException("Missing ERPAUTH_MASTER_KEY or ERPAUTH_MASTER_KEY_FILE");

        byte[] key;
        try
        {
            key = Convert.FromBase64String(keyB64);
        }
        catch
        {
            throw new InvalidOperationException("ERPAUTH_MASTER_KEY must be Base64.");
        }
        if (key.Length != 32) // AES-256
            throw new InvalidOperationException("ERPAUTH_MASTER_KEY must be 32 bytes (AES-256).");

        return key;
    }

    /// <summary>Lee un bloque <c>{Ciphertext, Nonce, Tag}</c> de una sección de configuración. Devuelve
    /// <c>null</c> si la sección no existe **o** si sus tres campos están vacíos/en blanco — los
    /// placeholders commiteables (<c>"Ciphertext": ""</c>, etc.) cuentan como "no configurado" (no-op),
    /// nunca como bloque a descifrar (que daría un nonce de 0 bytes y un crash engañoso). Sólo devuelve
    /// las tres cadenas cuando las tres están presentes y no vacías → en ese caso sí se descifra (y si
    /// falta/invalida la master key, se lanza el fail-fast claro).</summary>
    public static (string Ciphertext, string Nonce, string Tag)? TryGetEncryptedBlock(IConfigurationSection section)
    {
        if (!section.Exists()) return null;
        var ct = section["Ciphertext"];
        var nonce = section["Nonce"];
        var tag = section["Tag"];
        if (string.IsNullOrWhiteSpace(ct) || string.IsNullOrWhiteSpace(nonce) || string.IsNullOrWhiteSpace(tag))
            return null;
        return (ct!, nonce!, tag!);
    }

    /// <summary>Descifra <c>{Ciphertext, Nonce, Tag}</c> (base64) con la master key dada. Tag de 16
    /// bytes, sin AAD, plaintext UTF-8. (Inverso exacto del helper de cifrado.)</summary>
    public static string DecryptFromBase64(byte[] key, string ciphertextB64, string nonceB64, string tagB64)
    {
        var ct = Convert.FromBase64String(ciphertextB64);
        var nonce = Convert.FromBase64String(nonceB64);
        var tag = Convert.FromBase64String(tagB64);

        const int TagSize = 16; // Tamaño estándar de tag en bytes para AES-GCM
        var plaintext = new byte[ct.Length];
        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, ct, tag, plaintext, associatedData: null);
        return Encoding.UTF8.GetString(plaintext);
    }
}