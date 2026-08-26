using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Dtd.Infrastructure.Configuration;

/// <summary>
/// Descifra <c>Database:Password_Enc</c> (AES-256-GCM) con la **misma master key** que
/// <see cref="ErpAuthPostConfigure"/> (<c>ERPAUTH_MASTER_KEY</c>, resuelta por
/// <see cref="EncryptedSecretCipher"/>) e inyecta la contraseña en
/// <see cref="DatabaseOptions.ConnectionString"/> vía <see cref="NpgsqlConnectionStringBuilder"/>, para
/// que la contraseña de PostgreSQL nunca vaya en claro en el repo. El resto de la connection string
/// (host, puerto, db, usuario) no es secreto y queda committable en <c>appsettings.json</c>.
/// <para>Si existe el bloque cifrado pero falta/invalida la master key, lanza al arrancar (fail-fast).
/// Si no hay bloque, deja la connection string tal cual (modo trivial: la contraseña podría ir
/// inline o por otro mecanismo — no recomendado en prod).</para>
/// </summary>
public sealed class DatabaseAuthPostConfigure : IPostConfigureOptions<DatabaseOptions>
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<DatabaseAuthPostConfigure> _logger;

    public DatabaseAuthPostConfigure(IConfiguration cfg, ILogger<DatabaseAuthPostConfigure> logger)
    {
        _cfg = cfg;
        _logger = logger;
    }

    public void PostConfigure(string? name, DatabaseOptions opt)
    {
        // Si no hay bloque cifrado (o está vacío: placeholders commiteables), no hay nada que hacer
        // (la connection string se usa tal cual). Sólo se descifra un bloque con los tres campos rellenos.
        var enc = EncryptedSecretCipher.TryGetEncryptedBlock(_cfg.GetSection("Database:Password_Enc"));
        if (enc is null) return;

        var key = EncryptedSecretCipher.ResolveMasterKey(_cfg);
        var password = EncryptedSecretCipher.DecryptFromBase64(
            key, enc.Value.Ciphertext, enc.Value.Nonce, enc.Value.Tag);

        // Se sobrescribe/inyecta el Password sin tocar el resto de la connection string.
        var csb = new NpgsqlConnectionStringBuilder(opt.ConnectionString) { Password = password };
        opt.ConnectionString = csb.ConnectionString;

        _logger.LogDebug("DatabaseOptions.ConnectionString password injected via encrypted configuration.");
    }
}