using System.Security.Cryptography;
using System.Text;
using Dtd.Infrastructure.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dtd.Infrastructure.Tests;

public class ErpAuthPostConfigureTests
{
    // Misma convención que tools/Dtd.Tools.SecretCipher y que la otra app del grupo: AES-256-GCM,
    // key de 32 bytes, nonce de 12, tag de 16, sin AAD, plaintext UTF-8, todo base64.
    private static readonly byte[] Key = RandomNumberGenerator.GetBytes(32);
    private static readonly string KeyB64 = Convert.ToBase64String(Key);

    private static (string Ciphertext, string Nonce, string Tag) Encrypt(string plaintext)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var pt = Encoding.UTF8.GetBytes(plaintext);
        var ct = new byte[pt.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(Key, 16);
        aes.Encrypt(nonce, pt, ct, tag, associatedData: null);
        return (Convert.ToBase64String(ct), Convert.ToBase64String(nonce), Convert.ToBase64String(tag));
    }

    private static IConfiguration BuildConfig(string? keyB64, (string C, string N, string T) blob)
    {
        var dict = new Dictionary<string, string?>
        {
            ["Erp:ClientSecret_Enc:Ciphertext"] = blob.C,
            ["Erp:ClientSecret_Enc:Nonce"] = blob.N,
            ["Erp:ClientSecret_Enc:Tag"] = blob.T,
        };
        if (keyB64 is not null)
        {
            dict["ERPAUTH_MASTER_KEY"] = keyB64;
        }
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    private static ErpAuthPostConfigure Create(IConfiguration cfg) =>
        new(cfg, NullLogger<ErpAuthPostConfigure>.Instance);

    [Fact]
    public void PostConfigure_descifra_el_client_secret_y_lo_rellena()
    {
        // Un ciphertext producido con la misma convención (helper / otra app) se descifra aquí tal cual.
        var blob = Encrypt("mi-client-secret-001");
        var opt = new ErpOptions();

        Create(BuildConfig(KeyB64, blob)).PostConfigure(null, opt);

        opt.ClientSecret.Should().Be("mi-client-secret-001");
    }

    [Fact]
    public void PostConfigure_falla_si_la_master_key_no_es_de_32_bytes()
    {
        var blob = Encrypt("x");
        var shortKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)); // 16 bytes, no 32

        var act = () => Create(BuildConfig(shortKey, blob)).PostConfigure(null, new ErpOptions());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PostConfigure_falla_si_la_master_key_no_es_base64_valida()
    {
        var blob = Encrypt("x");

        var act = () => Create(BuildConfig("esto-no-es-base64!!", blob)).PostConfigure(null, new ErpOptions());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PostConfigure_no_hace_nada_si_no_hay_bloque_cifrado()
    {
        // p.ej. modo mock: no hay ClientSecret_Enc → no se toca ClientSecret (se evita fallar al arrancar).
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ERPAUTH_MASTER_KEY"] = KeyB64 })
            .Build();
        var opt = new ErpOptions();

        Create(cfg).PostConfigure(null, opt);

        opt.ClientSecret.Should().BeNull();
    }

    [Fact]
    public void PostConfigure_no_hace_nada_si_el_bloque_esta_vacio_placeholders()
    {
        // El estado commiteado de appsettings.json tiene el bloque con "" (placeholders). Eso NO debe
        // intentar descifrar (daría un nonce de 0 bytes → crash "nonce not a valid size") ni fallar por
        // missing key: cuenta como "no configurado" → no-op, aunque la master key sí esté presente.
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ERPAUTH_MASTER_KEY"] = KeyB64,
            ["Erp:ClientSecret_Enc:Ciphertext"] = "",
            ["Erp:ClientSecret_Enc:Nonce"] = "",
            ["Erp:ClientSecret_Enc:Tag"] = "",
        }).Build();
        var opt = new ErpOptions();

        var act = () => Create(cfg).PostConfigure(null, opt);

        act.Should().NotThrow();
        opt.ClientSecret.Should().BeNull();
    }
}