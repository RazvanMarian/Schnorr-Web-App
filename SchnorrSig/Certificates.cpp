#include <iostream>
#include <openssl/x509.h>
#include <openssl/x509v3.h>
#include <openssl/evp.h>

void getOpenSSLError()
{
    BIO *bio = BIO_new(BIO_s_mem());
    ERR_print_errors(bio);
    char *buf;
    size_t len = BIO_get_mem_data(bio, &buf);
    std::string ret(buf, len);
    BIO_free(bio);

    std::cout << ret << std::endl;
}

X509 *Create_Certificate(EC_KEY *key, bool flag)
{

    int ret;

    // Setare cheie publica cerere certificat
    EVP_PKEY *pKey = EVP_PKEY_new();
    ret = EVP_PKEY_set1_EC_KEY(pKey, key);
    if (ret != 1)
    {
        printf("eroare setare cheie evp!\n");
        return NULL;
    }

    // Incarcare certificat CA
    FILE *fp;
    if (!(fp = fopen("/home/razvan/certs/myCA.pem", "r")))
    {
        std::cout << "Eroare la citirea certificatului CA-ului" << std::endl;
        return NULL;
    }

    X509 *cacert;
    if (!(cacert = PEM_read_X509(fp, NULL, NULL, NULL)))
    {
        std::cout << "Eroare incarcare certificat CA in memorie!" << std::endl;
        return NULL;
    }
    fclose(fp);

    // Importare cheie privata CA
    EVP_PKEY *ca_privkey = EVP_PKEY_new();

    if (!(fp = fopen("/home/razvan/certs/myCA.key", "r")))
    {
        std::cout << "Eroare deschidere fisier cheie CA" << std::endl;
        return NULL;
    }

    if (!(ca_privkey = PEM_read_PrivateKey(fp, NULL, NULL, (void *)"gigel123")))
    {
        std::cout << "Eroare citire cheie privata CA" << std::endl;
        return NULL;
    }

    fclose(fp);

    // Creare certificat
    X509 *newcert;
    if (!(newcert = X509_new()))
    {
        std::cout << "Eroare alocare certificat nou!" << std::endl;
        return NULL;
    }

    if (X509_set_version(newcert, 2) != 1)
    {
        std::cout << "Eroare setare versiune certificat" << std::endl;
        return NULL;
    }

    // Setare serial number certificat
    ASN1_INTEGER *aserial = ASN1_INTEGER_new();
    ASN1_INTEGER_set(aserial, 1);
    if (!X509_set_serialNumber(newcert, aserial))
    {
        std::cout << "Eroare setare serie certificat!" << std::endl;
        return NULL;
    }

    // Extragere subject name din request
    X509_NAME *name;
    if (!(name = X509_get_subject_name(newcert)))
        std::cout << "Eroare preluare subiect din cerere!" << std::endl;

    // Setare subject name in certificatul nou
    if (X509_set_subject_name(newcert, name) != 1)
    {
        std::cout << "Eroare setare subject name certificat!" << std::endl;
        return NULL;
    }

    // Extragere subject name din certificatul CA
    if (!(name = X509_get_subject_name(cacert)))
    {
        std::cout << "Eroare preluare subject name de la certificatul CA!" << std::endl;
        return NULL;
    }

    // Setare issuer name
    if (X509_set_issuer_name(newcert, name) != 1)
    {
        std::cout << "Eroare setare issuer name!" << std::endl;
        return NULL;
    }

    // Setare cheie publica certificat
    if (X509_set_pubkey(newcert, pKey) != 1)
    {
        std::cout << "Eroare setare cheie publica certificat!" << std::endl;
        return NULL;
    }

    // Setare valabilitate 365 de zile
    if (!(X509_gmtime_adj(X509_get_notBefore(newcert), 0)))
    {
        std::cout << "Eroare setare start date!" << std::endl;
        return NULL;
    }

    if (!(X509_gmtime_adj(X509_get_notAfter(newcert), 31536000L)))
    {
        std::cout << "Eroare setare expiration date!" << std::endl;
        return NULL;
    }

    // Adaugare extensie x509V3
    X509V3_CTX ctx;
    X509V3_set_ctx(&ctx, cacert, newcert, NULL, NULL, 0);
    X509_EXTENSION *ext;

    // Semnarea certificatului cu cheia privata a CA-ului
    EVP_MD const *digest = NULL;
    digest = EVP_sha256();

    if (!X509_sign(newcert, ca_privkey, digest))
    {
        std::cout << "Eroare setare digest type!" << std::endl;
        return NULL;
    }

    // Salvare certificat
    if (flag == true)
    {
        BIO *output = BIO_new_file((const char *)"/home/razvan/certificates/cert.pem", "wb");

        ret = PEM_write_bio_X509(output, newcert);
        if (ret == 0)
        {
            std::cout << "Eroare la scriere in fisier!" << std::endl;
            return NULL;
        }

        BIO_free_all(output);
    }

    return newcert;
}

int Read_Public_Key_Certificate(EC_KEY **key, const char *certificatePath)
{
    OpenSSL_add_all_algorithms();
    ERR_load_BIO_strings();
    ERR_load_crypto_strings();

    BIO *certbio = BIO_new(BIO_s_file());
    X509 *cert = NULL;
    EVP_PKEY *pkey = NULL;

    int ret = BIO_read_filename(certbio, certificatePath);
    if (!(cert = PEM_read_bio_X509(certbio, NULL, 0, NULL)))
    {
        printf("Error loading cert into memory\n");
        return -1;
    }

    if ((pkey = X509_get_pubkey(cert)) == NULL)
    {
        printf("Error getting public key from certificate\n");
        return -1;
    }

    int result = EVP_PKEY_base_id(pkey);
    if (result != EVP_PKEY_EC)
    {
        printf("The certificate does not contain an EC_KEY type of key\n");
        return -1;
    }

    *key = EVP_PKEY_get1_EC_KEY(pkey);
    if (*key == NULL)
    {
        printf("Error extracting the EC_KEY from the EVP_PKEY\n");
        return -1;
    }

    BIO_free_all(certbio);
    X509_free(cert);
    EVP_PKEY_free(pkey);

    return 0;
}

int Get_Public_Key_From_Certificate(EC_KEY **key, X509 *cert)
{

    OpenSSL_add_all_algorithms();
    ERR_load_BIO_strings();
    ERR_load_crypto_strings();

    EVP_PKEY *pkey = NULL;

    if ((pkey = X509_get_pubkey(cert)) == NULL)
    {
        printf("Error getting public key from certificate\n");
        return -1;
    }

    int result = EVP_PKEY_base_id(pkey);
    if (result != EVP_PKEY_EC)
    {
        printf("The certificate does not contain an EC_KEY type of key\n");
        return -1;
    }

    *key = EVP_PKEY_get1_EC_KEY(pkey);
    if (*key == NULL)
    {
        printf("Error extracting the EC_KEY from the EVP_PKEY\n");
        return -1;
    }

    X509_free(cert);
    EVP_PKEY_free(pkey);

    return 0;
}
