#include <openssl/schnorr.h>
#include <vector>
#include <string>
#include <iostream>
#include <iterator>
#include "Certificates.cpp"
#include "SignatureOutput.cpp"

extern "C"
{
    int Multiple_Sign(const char *hash, const char *keys[], int signersNumber)
    {

        EC_KEY *SCHNORR_keys[signersNumber];
        int res;
        for (int i = 0; i < signersNumber; i++)
        {

            res = SCHNORR_read_private_key(&SCHNORR_keys[i], keys[i]);
            if (res != 0)
            {
                std::cout << "Erorr reading the private key!" << std::endl;
                return -1;
            }
        }

        SCHNORR_SIG *sig = SCHNORR_SIG_new();
        res = SCHNORR_multiple_sign(SCHNORR_keys, signersNumber, hash, SHA256_DIGEST_LENGTH, sig);
        if (res != 0)
        {
            std::cout << "Eroare la crearea semnaturii" << std::endl;
            return -1;
        }

        res = SCHNORR_multiple_verify(SCHNORR_keys, signersNumber, hash, SHA256_DIGEST_LENGTH, sig);
        if (res != 0)
        {
            std::cout << "Eroare la verificarea semnaturii" << std::endl;
            return -1;
        }

        res = SCHNORR_write_signature(sig, "/home/razvan/signatures/signature.plain");
        if (res != 0)
        {
            std::cout << "Eroare la scrierea semnaturii in fisier" << std::endl;
            return -1;
        }

        EC_KEY *pKey = SCHNORR_generate_aggregate_public_key(SCHNORR_keys, signersNumber);
        if (pKey == NULL)
        {
            std::cout << "Eroare la generarea cheii publice agregate" << std::endl;
            return -1;
        }

        EC_KEY *privateKey = SCHNORR_generate_aggregate_private_key(SCHNORR_keys, signersNumber);
        if (pKey == NULL)
        {
            std::cout << "Eroare la generarea cheii private agregate" << std::endl;
            return -1;
        }

        X509 *cert = Create_Certificate(privateKey, pKey);
        if (cert == NULL)
        {
            std::cout << "Eroare la creare certificat!" << std::endl;
            return -1;
        }

        if (write_signature(sig, cert) != 0)
        {
            std::cout << "Eroare la scrierea semnaturii!" << std::endl;
            return -1;
        }

        return 0;
    }

    int Generate(const char *privateFilename, const char *publicFilename)
    {
        EC_KEY *key;
        int res = SCHNORR_generate_key(&key);
        if (res != 0)
        {
            std::cout << "Eroare la generarea cheii !" << std::endl;
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

    int Sign_Document(const char *hash, const char *privateFilename, const char *publicFilename)
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

        X509 *cert = Create_Certificate(sign_key, verify_key);
        if (cert == NULL)
        {
            std::cout << "Eroare la creare certificat!" << std::endl;
            return -1;
        }

        if (write_signature(sig, cert) != 0)
        {
            std::cout << "Eroare la scrierea semnaturii!" << std::endl;
            return -1;
        }

        return 0;
    }

    int Generate_Certificate(const char *privateFilename, const char *publicFilename)
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

        X509 *cert = Create_Certificate(private_key, public_key);
        if (cert == NULL)
        {
            std::cout << "Eroare la creare certificat !" << std::endl;
            return 1;
        }

        EC_KEY_free(private_key);
        EC_KEY_free(public_key);

        return 0;
    }

    int Verify_File(const char *hash, const char *signatureFileName)
    {
        EC_KEY *key = NULL;

        X509 *cert = X509_new();
        SCHNORR_SIG *sig = SCHNORR_SIG_new();
        int res = read_signature(signatureFileName, cert, sig);
        if (res != 0)
        {
            return -1;
        }

        res = Get_Public_Key_From_Certificate(&key, cert);
        if (res != 0)
        {
            return -1;
        }

        res = SCHNORR_verify(key, hash, SHA256_DIGEST_LENGTH, sig);
        if (res != 0)
        {
            return -1;
        }

        SCHNORR_SIG_free(sig);
        return res;
    }
}