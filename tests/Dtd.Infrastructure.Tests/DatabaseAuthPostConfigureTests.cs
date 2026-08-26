using System.Security.Cryptography;
using System.Text;
using Dtd.Infrastructure.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dtd.Infrastructure.Tests;

public class DatabaseAuthPostConfigureTests
{
    private static readonly byte[] Key = RandomNumberGenerator.GetBytes(32);
    private static readonly string KeyB64 = Convert.ToBase64String(Key);

    private static (string C, string N, string T) Encrypt(string plaintext)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var pt = Encoding.UTF8.GetBytes(plaintext);
        var ct = new byte[pt.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(Key, 16);
        aes.Encrypt(nonce, pt, ct, tag, associatedData: null);
        return (Convert.ToBase64String(ct), Convert.ToBase64String(nonce), Convert.ToBase64String(tag));
    }

    private const string BaseConnectionString = "Host=postgresql01.gruposoledad.com;Port=5432;Database=dtd;Username=dtd_user";

    private static IConfiguration BuildConfig(string? keyB64, (string C, string N, string T) blob)
    {
        var dict = new Dictionary<string, string?>
        {
            ["Database:ConnectionString"] = BaseConnectionString,
            ["Database:Password_Enc:Ciphertext"] = blob.C,
            ["Database:Password_Enc:Nonce"] = blob.N,
            ["Database:Password_Enc:Tag"] = blob.T,
        };
        if (keyB64 is not null)
        {
            dict["ERPAUTH_MASTER_KEY"] = keyB64;
        }
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    private static DatabaseAuthPostConfigure Create(IConfiguration cfg) =>
        new(cfg, NullLogger<DatabaseAuthPostConfigure>.Instance);

    [Fact]
    public void PostConfigure_inyecta_la_contrasena_descifrada_en_la_connection_string()
    {
        var blob = Encrypt("Docuten26!");
        var opt = new DatabaseOptions { ConnectionString = BaseConnectionString };

        Create(BuildConfig(KeyB64, blob)).PostConfigure(null, opt);

        // La contraseña descifrada aparece en la connection string; la master key usada es la misma
        // que para el client_secret del ERP (ERPAUTH_MASTER_KEY).
        opt.ConnectionString.Should().Contain("Docuten26!");
        opt.ConnectionString.Should().StartWith("Host=postgresql01.gruposoledad.com");
        // No queda rastro de la contraseña en claro en la base original (se inyecta sólo en memoria).
        opt.ConnectionString.Should().NotBe(BaseConnectionString);
    }

    [Fact]
    public void PostConfigure_no_toca_la_connection_string_si_no_hay_bloque_cifrado()
    {
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = BaseConnectionString,
                ["ERPAUTH_MASTER_KEY"] = KeyB64
            })
            .Build();
        var opt = new DatabaseOptions { ConnectionString = BaseConnectionString };

        Create(cfg).PostConfigure(null, opt);

        opt.ConnectionString.Should().Be(BaseConnectionString);
    }

    [Fact]
    public void PostConfigure_no_toca_la_connection_string_si_el_bloque_esta_vacio_placeholders()
    {
        // El estado commiteado de appsettings.json tiene Password_Enc con "" (placeholders). Eso NO debe
        // intentar descifrar (nonce de 0 bytes → crash engañoso) ni fallar por missing key: es "no
        // configurado" → la connection string se usa tal cual, aunque la master key sí esté presente.
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:ConnectionString"] = BaseConnectionString,
            ["ERPAUTH_MASTER_KEY"] = KeyB64,
            ["Database:Password_Enc:Ciphertext"] = "",
            ["Database:Password_Enc:Nonce"] = "",
            ["Database:Password_Enc:Tag"] = "",
        }).Build();
        var opt = new DatabaseOptions { ConnectionString = BaseConnectionString };

        var act = () => Create(cfg).PostConfigure(null, opt);

        act.Should().NotThrow();
        opt.ConnectionString.Should().Be(BaseConnectionString);
    }

    [Fact]
    public void PostConfigure_falla_si_la_master_key_no_es_de_32_bytes()
    {
        var blob = Encrypt("x");
        var shortKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

        var act = () => Create(BuildConfig(shortKey, blob)).PostConfigure(null, new DatabaseOptions());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PostConfigure_y_ErpAuth_comparten_la_misma_master_key_y_formato()
    {
        // Un mismo ciphertext (producido con la master key compartida) descifra el mismo valor tanto
        // para el client_secret del ERP como para la contraseña de BD: confirma el formato único.
        var blob = Encrypt("shared-secret");

        var erpOpt = new ErpOptions();
        var erpCfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ERPAUTH_MASTER_KEY"] = KeyB64,
            ["Erp:ClientSecret_Enc:Ciphertext"] = blob.C,
            ["Erp:ClientSecret_Enc:Nonce"] = blob.N,
            ["Erp:ClientSecret_Enc:Tag"] = blob.T,
        }).Build();
        new ErpAuthPostConfigure(erpCfg, NullLogger<ErpAuthPostConfigure>.Instance).PostConfigure(null, erpOpt);

        var dbOpt = new DatabaseOptions { ConnectionString = BaseConnectionString };
        new DatabaseAuthPostConfigure(BuildConfig(KeyB64, blob), NullLogger<DatabaseAuthPostConfigure>.Instance)
            .PostConfigure(null, dbOpt);

        erpOpt.ClientSecret.Should().Be("shared-secret");
        dbOpt.ConnectionString.Should().Contain("shared-secret");
    }
}