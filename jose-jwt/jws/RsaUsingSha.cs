using System;
using System.Security.Cryptography;

namespace Jose
{
    public class RsaUsingSha : IJwsAlgorithm
    {
        private readonly string hashMethod;

        public RsaUsingSha(string hashMethod)
        {
            this.hashMethod = hashMethod;
        }

        public byte[] Sign(byte[] securedInput, object key)
        {
#if NET40
            var privateKey = Ensure.Type<RSA>(key, "RsaUsingSha alg expects key to be of AsymmetricAlgorithm type.");

            using (var sha = HashAlgorithm)
            {
                var pkcs1 = new RSAPKCS1SignatureFormatter(privateKey);
                pkcs1.SetHashAlgorithm(hashMethod);

                return pkcs1.CreateSignature(sha.ComputeHash(securedInput));
            }

#elif NETSTANDARD || NET461
            var privateKey = Ensure.Type<RSA>(key, "RsaUsingSha alg expects key to be of RSA type.");

            return privateKey.SignData(securedInput, HashAlgorithm, RSASignaturePadding.Pkcs1);
#endif
        }

        public bool Verify(byte[] signature, byte[] securedInput, object key)
        {
#if NET40
            using (var sha = HashAlgorithm)
            {
                var publicKey = Ensure.Type<AsymmetricAlgorithm>(key, "RsaUsingSha alg expects key to be of AsymmetricAlgorithm type.");

                byte[] hash = sha.ComputeHash(securedInput);
                var pkcs1 = new RSAPKCS1SignatureDeformatter(publicKey);
                pkcs1.SetHashAlgorithm(hashMethod);

                return pkcs1.VerifySignature(hash, signature);
            }
#elif NETSTANDARD || NET461
            var publicKey = Ensure.Type<RSA>(key, "RsaUsingSha alg expects key to be of RSA type.");

            return publicKey.VerifyData(securedInput, signature, HashAlgorithm, RSASignaturePadding.Pkcs1);
#endif
        }

#if NET40
        private HashAlgorithm HashAlgorithm
        {
            get
            {
                if (hashMethod.Equals("SHA256"))
                    return new SHA256CryptoServiceProvider();
                else if (hashMethod.Equals("SHA384"))
                    return new SHA384CryptoServiceProvider();
                else if (hashMethod.Equals("SHA512"))
                    return new SHA512CryptoServiceProvider();
                else
                    throw new ArgumentException("Unsupported hashing algorithm: '{0}'", hashMethod);
            }
        }
#elif NETSTANDARD || NET461
        private HashAlgorithmName HashAlgorithm
        {
            get
            {
                if (hashMethod.Equals("SHA256"))
                    return HashAlgorithmName.SHA256;
                else if (hashMethod.Equals("SHA384"))
                    return HashAlgorithmName.SHA384;
                else if (hashMethod.Equals("SHA512"))
                    return HashAlgorithmName.SHA512;
                else
                    throw new ArgumentException("Unsupported hashing algorithm: '{0}'", hashMethod);
            }
        }
#endif
    }
}