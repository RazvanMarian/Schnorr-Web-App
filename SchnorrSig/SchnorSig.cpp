#include <openssl/schnorr.h>
#include <vector>
#include <string>
#include <iostream>
#include <iterator>
#include "Certificates.cpp"
#include "SignatureOutput.cpp"

extern "C"
{
    int MultipleSign(const char *hash, const char *keys[], int signersNumber)
    {

        EC_KEY *SCHNORR_keys[signersNumber];
        X509 *certs[signersNumber];
        int res;
        for (int i = 0; i < signersNumber; i++)
        {

            res = SCHNORR_read_private_key(&SCHNORR_keys[i], keys[i]);
            if (res != 0)
            {
                std::cout << "Erorr reading the private key!" << std::endl;
                return -1;
            }
            certs[i] = Create_Certificate(SCHNORR_keys[i], false);
        }

        SCHNORR_SIG *sig = SCHNORR_SIG_new();
        res = SCHNORR_multiple_sign(SCHNORR_keys, signersNumber, hash, SHA256_DIGEST_LENGTH, sig);
        if (res != 0)
        {
            std::cout << "Eroare la crearea semnaturii!" << std::endl;
            return -1;
        }

        res = SCHNORR_multiple_verify(SCHNORR_keys, signersNumber, hash, SHA256_DIGEST_LENGTH, sig);
        if (res != 0)
        {
            std::cout << "Eroare la verificarea semnaturii" << std::endl;
            return -1;
        }

        SCHNORR_SIGNED_DATA *signed_data = SCHNORR_create_pkcs7(SCHNORR_keys, certs, signersNumber, sig);
        if (signed_data == NULL)
        {
            std::cout << "Eroare creare strucutra pkcs7 semnatura" << std::endl;
            return -1;
        }

        res = write_schnorr_signed_data_asn1(signed_data, "/home/razvan/signatures/signature.bin");
        if (res != 0)
        {
            std::cout << "Eroare la scrierea semnaturii in fisier!" << std::endl;
            return -1;
        }

        return 0;
    }

    int GenerateKeyPair(const char *privateFilename, const char *publicFilename)
    {
        EC_KEY *key;
        int res = SCHNORR_generate_key(&key);
        if (res != 0)
        {
            std::cout << "Eroare la generarea cheii!" << std::endl;
            return res;
        }

        res = SCHNORR_write_private_key(key, privateFilename);
        if (res != 0)
        {
            std::cout << "Eroare la scrierea cheii in fisier !" << std::endl;
            return res;
        }

        res = SCHNORR_write_public_key(key, publicFilename);
        if (res != 0)
        {
            std::cout << "Eroare la scrierea cheii in fisier!" << std::endl;
            return res;
        }

        return res;
    }

    int Sign(const char *hash, const char *privateFilename, const char *publicFilename)
    {
        EC_KEY *sign_key;
        EC_KEY *verify_key;
        SCHNORR_SIG *sig = SCHNORR_SIG_new();

        int res = SCHNORR_read_private_key(&sign_key, privateFilename);
        if (res != 0)
        {
            std::cout << "Eroare la citirea cheii private!" << std::endl;
            return -1;
        }

        res = SCHNORR_sign(sign_key, hash, SHA256_DIGEST_LENGTH, sig);
        if (res != 0)
        {
            std::cout << "Eroare la semnare!" << std::endl;
            return -1;
        }

        res = SCHNORR_read_public_key(&verify_key, publicFilename);
        if (res != 0)
        {
            std::cout << "Eroare la citirea cheii private!" << std::endl;
            return -1;
        }

        EC_KEY **key = (EC_KEY **)malloc(sizeof(EC_KEY *));
        key[0] = EC_KEY_new();
        const EC_POINT *point = EC_KEY_get0_public_key(verify_key);
        const EC_GROUP *group = EC_KEY_get0_group(verify_key);
        if (point == NULL)
        {
            std::cout << "eroare" << std::endl;
            return -1;
        }

        if (EC_POINT_is_at_infinity(group, point))
        {
            std::cout << "eroare punct la infinitate" << std::endl;
            return -1;
        }
        EC_KEY_set_group(key[0], group);
        int ret = EC_KEY_set_public_key(key[0], point);
        const BIGNUM *number = EC_KEY_get0_private_key(sign_key);
        ret = EC_KEY_set_private_key(key[0], number);

        X509 *cert[1];
        cert[0] = Create_Certificate(key[0], false);
        if (cert == NULL)
        {
            std::cout << "Eroare la creare certificat!" << std::endl;
            return -1;
        }

        SCHNORR_SIGNED_DATA *signed_data = SCHNORR_create_pkcs7(key, cert, 1, sig);
        write_schnorr_signed_data_asn1(signed_data, "/home/razvan/signatures/signature.bin");

        return 0;
    }

    int GenerateCertificate(const char *privateFilename, const char *publicFilename)
    {
        EC_KEY *private_key;
        EC_KEY *public_key;

        int res = SCHNORR_read_private_key(&private_key, privateFilename);
        if (res != 0)
        {
            std::cout << "Eroare la citirea cheii private !" << std::endl;
            return 1;
        }

        res = SCHNORR_read_public_key(&public_key, publicFilename);
        if (res != 0)
        {
            std::cout << "Eroare la citirea cheii private!" << std::endl;
            return 1;
        }

        EC_KEY *key = EC_KEY_new();
        const EC_POINT *point = EC_KEY_get0_public_key(public_key);
        const EC_GROUP *group = EC_KEY_get0_group(public_key);
        if (point == NULL)
        {
            std::cout << "eroare" << std::endl;
        }

        if (EC_POINT_is_at_infinity(group, point))
        {
            std::cout << "eroare punct la infinitate" << std::endl;
        }
        EC_KEY_set_group(key, group);
        int ret = EC_KEY_set_public_key(key, point);
        const BIGNUM *number = EC_KEY_get0_private_key(private_key);
        ret = EC_KEY_set_private_key(key, number);

        X509 *cert = Create_Certificate(key, true);
        if (cert == NULL)
        {
            std::cout << "Eroare la creare certificat !" << std::endl;
            return 1;
        }

        EC_KEY_free(private_key);
        EC_KEY_free(public_key);

        return 0;
    }

    int Verify(const char *hash, const char *signatureFileName)
    {

        SCHNORR_SIGNED_DATA *data;
        read_schnorr_signed_data_asn1(&data, signatureFileName);

        STACK_OF(X509) *x509_stack = SCHNORR_get_signers_certificates(data);
        if (x509_stack == NULL)
        {
            printf("eroare preluare certificate\n");
            return -1;
        }

        SCHNORR_SIG *signature = SCHNORR_get_signature(data);
        if (signature == NULL)
        {
            printf("eroare preluare semnatura\n");
            return -1;
        }
        int signers_number = sk_X509_num(x509_stack);

        EC_KEY **keys = (EC_KEY **)malloc(sizeof(EC_KEY *) * signers_number);
        for (int i = 0; i < signers_number; i++)
        {
            X509 *cert = sk_X509_value(x509_stack, i);
            EVP_PKEY *pkey = X509_get_pubkey(cert);

            keys[i] = EVP_PKEY_get0_EC_KEY(pkey);
        }

        if (SCHNORR_multiple_verify(keys, signers_number, hash, SHA256_DIGEST_LENGTH, signature) != 0)
        {
            printf("eroare verificare\n");
            return -1;
        }

        return 0;
    }
}