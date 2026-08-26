using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dtd.Infrastructure.Configuration;

/// <summary>
/// Descifra <c>Erp:ClientSecret_Enc</c> (AES-256-GCM) y rellena <see cref="ErpOptions.ClientSecret"/>
/// en memoria, con la master key compartida <c>ERPAUTH_MASTER_KEY</c> (resuelta por
/// <see cref="EncryptedSecretCipher"/>). Es una réplica fiel de la clase homónima usada en otra app
/// del grupo: mismo formato AES-GCM (tag 16B, sin AAD, UTF-8), mismos env vars
/// <c>ERPAUTH_MASTER_KEY</c>/<c>ERPAUTH_MASTER_KEY_FILE</c> → el mismo ciphertext y el mismo montaje
/// de Secret sirven aquí.
/// <para>El <b>ciphertext</b> (<c>Ciphertext</c>/<c>Nonce</c>/<c>Tag</c>, base64) vive en
/// <c>appsettings.json</c> (committable: es ciphertext). La <b>master key</b> <b>nunca</b> se
/// commitea: se inyecta por <c>ERPAUTH_MASTER_KEY</c> (config / env var — user-secrets en dev) o
/// <c>ERPAUTH_MASTER_KEY_FILE</c> (fichero, montaje de Secret de k8s). El <c>client_secret</c> es
/// común a todas las empresas.</para>
/// <para>Si existe el bloque cifrado pero falta/invalida la master key, lanza al arrancar (fail-fast
/// vía <c>ValidateOnStart</c>). Si no hay bloque (p.ej. <c>UseMock=true</c>), no hace nada.</para>
/// </summary>
public sealed class ErpAuthPostConfigure : IPostConfigureOptions<ErpOptions>
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<ErpAuthPostConfigure> _logger;

    public ErpAuthPostConfigure(IConfiguration cfg, ILogger<ErpAuthPostConfigure> logger)
    {
        _cfg = cfg;
        _logger = logger;
    }

    public void PostConfigure(string? name, ErpOptions opt)
    {
        // Si no hay bloque cifrado (o está vacío: placeholders commiteables), no hay nada que hacer.
        // Sólo se descifra un bloque con los tres campos rellenos (p.ej. UseMock=true deja el bloque vacío).
        var enc = EncryptedSecretCipher.TryGetEncryptedBlock(_cfg.GetSection("Erp:ClientSecret_Enc"));
        if (enc is null) return;

        var key = EncryptedSecretCipher.ResolveMasterKey(_cfg);
        opt.ClientSecret = EncryptedSecretCipher.DecryptFromBase64(
            key, enc.Value.Ciphertext, enc.Value.Nonce, enc.Value.Tag);

        _logger.LogDebug("ErpOptions.ClientSecret set via encrypted configuration.");
    }
}