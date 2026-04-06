using System.IO;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace Negocio.Config
{
    public static class CertificateHelper
    {
        public static X509Certificate2 LoadCertificateWithKey(string certPath, string keyPath, string password)
        {
            // Carrega o certificado PEM
            X509Certificate2 cert;
            using (var reader = File.OpenRead(certPath))
            {
                var pemReader = new PemReader(new StreamReader(reader));
                var certObject = pemReader.ReadObject();
                var bcCert = (Org.BouncyCastle.X509.X509Certificate)certObject;
                cert = new X509Certificate2(bcCert.GetEncoded());
            }

            // Carrega a chave privada PEM
            AsymmetricCipherKeyPair keyPair;
            using (var reader = new StreamReader(keyPath))
            {
                var pemReader = new PemReader(reader);
                keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
            }

            // Converte a chave BouncyCastle para RSA nativo do .NET
            var rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)keyPair.Private);
            var rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
            rsa.ImportParameters(rsaParams);

            // Associa a chave privada ao certificado
            var certWithKey = cert.CopyWithPrivateKey(rsa); // .NET 4.6.2+
            return new X509Certificate2(
                certWithKey.Export(X509ContentType.Pkcs12),
                password,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
        }
    }
}
