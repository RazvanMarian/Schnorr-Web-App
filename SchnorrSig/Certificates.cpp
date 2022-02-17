#include <iostream>
#include <openssl/x509.h>
#include <openssl/x509v3.h>

int Create_Certificate(EC_KEY *private_key, EC_KEY *public_key)
{
    OpenSSL_add_all_algorithms();
    ERR_load_BIO_strings();
    ERR_load_crypto_strings();

    // Setare versiune cerere certificat
    X509_REQ *x509_req = X509_REQ_new();
    int ret = X509_REQ_set_version(x509_req, 1);
    if (ret != 1)
    {
        std::cout << "Eroare setare versiune request" << std::endl;
        return -1;
    }

    // Setare subject cerere certificat
    X509_NAME *x509_name = X509_REQ_get_subject_name(x509_req);

    ret = X509_NAME_add_entry_by_txt(x509_name, "C", MBSTRING_ASC, (const unsigned char *)"RO", -1, -1, 0);
    if (ret != 1)
    {
        std::cout << "Eroare setare camp 1" << std::endl;
        return -1;
    }

    ret = X509_NAME_add_entry_by_txt(x509_name, "ST", MBSTRING_ASC, (const unsigned char *)"B", -1, -1, 0);
    if (ret != 1)
    {
        std::cout << "Eroare setare camp 2" << std::endl;
        return -1;
    }

    ret = X509_NAME_add_entry_by_txt(x509_name, "L", MBSTRING_ASC, (const unsigned char *)"Bucharest", -1, -1, 0);
    if (ret != 1)
    {
        std::cout << "Eroare setare camp 3" << std::endl;
        return -1;
    }

    ret = X509_NAME_add_entry_by_txt(x509_name, "O", MBSTRING_ASC, (const unsigned char *)"Company Inc.", -1, -1, 0);
    if (ret != 1)
    {
        std::cout << "Eroare setare camp 3" << std::endl;
        return -1;
    }

    ret = X509_NAME_add_entry_by_txt(x509_name, "CN", MBSTRING_ASC, (const unsigned char *)"Co. Inc.", -1, -1, 0);
    if (ret != 1)
    {
        std::cout << "Eroare setare camp 4" << std::endl;
        return -1;
    }

    // Setare cheie publica cerere certificat
    EVP_PKEY *pKey = EVP_PKEY_new();
    EVP_PKEY_assign_EC_KEY(pKey, public_key);

    ret = X509_REQ_set_pubkey(x509_req, pKey);
    if (ret != 1)
    {
        std::cout << "Eroare setare cheie publica cerere" << std::endl;
        return -1;
    }

    EVP_PKEY *pkey_private = EVP_PKEY_new();
    EVP_PKEY_assign_EC_KEY(pkey_private, private_key);
    ret = X509_REQ_sign(x509_req, pkey_private, EVP_sha256()); // return x509_req->signature->length
    if (ret <= 0)
    {
        std::cout << "Eroare semnare cerere" << std::endl;
        return -1;
    }

    // Incarcare certificat CA
    FILE *fp;
    if (!(fp = fopen("/home/razvan/certs/myCA.pem", "r")))
    {
        std::cout << "Eroare la citirea certificatului CA-ului" << std::endl;
        return -1;
    }

    X509 *cacert;
    if (!(cacert = PEM_read_X509(fp, NULL, NULL, NULL)))
    {
        std::cout << "Eroare incarcare certificat CA in memorie!" << std::endl;
        return -1;
    }
    fclose(fp);

    // Importare cheie privata CA
    EVP_PKEY *ca_privkey = EVP_PKEY_new();

    if (!(fp = fopen("/home/razvan/certs/myCA.key", "r")))
    {
        std::cout << "Eroare deschidere fisier cheie CA" << std::endl;
        return -1;
    }

    if (!(ca_privkey = PEM_read_PrivateKey(fp, NULL, NULL, (void *)"gigel123")))
    {
        std::cout << "Eroare citire cheie privata CA" << std::endl;
        return -1;
    }

    fclose(fp);

    // Creare certificat din request
    X509 *newcert;
    if (!(newcert = X509_new()))
    {
        std::cout << "Eroare alocare certificat nou!" << std::endl;
        return -1;
    }

    if (X509_set_version(newcert, 2) != 1)
    {
        std::cout << "Eroare setare versiune certificat" << std::endl;
        return -1;
    }

    // Setare serial number certificat
    ASN1_INTEGER *aserial = ASN1_INTEGER_new();
    ASN1_INTEGER_set(aserial, 1);
    if (!X509_set_serialNumber(newcert, aserial))
    {
        std::cout << "Eroare setare serie certificat!" << std::endl;
        return -1;
    }

    // Extragere subject name din request
    X509_NAME *name;
    if (!(name = X509_REQ_get_subject_name(x509_req)))
        std::cout << "Eroare preluare subiect din cerere!" << std::endl;

    // Setare subject name in certificatul nou
    if (X509_set_subject_name(newcert, name) != 1)
    {
        std::cout << "Eroare setare subject name certificat!" << std::endl;
        return -1;
    }

    // Extragere subject name din certificatul CA
    if (!(name = X509_get_subject_name(cacert)))
    {
        std::cout << "Eroare preluare subject name de la certificatul CA!" << std::endl;
        return -1;
    }

    // Setare issuer name
    if (X509_set_issuer_name(newcert, name) != 1)
    {
        std::cout << "Eroare setare issuer name!" << std::endl;
        return -1;
    }

    // Extragere cheie publica din request
    EVP_PKEY *req_pubkey;
    if (!(req_pubkey = X509_REQ_get_pubkey(x509_req)))
    {
        std::cout << "Eroare extragere cheie publica din request!" << std::endl;
        return -1;
    }

    /// Verificare semnatura request
    if (X509_REQ_verify(x509_req, req_pubkey) != 1)
    {
        std::cout << "Eroare verificare semnatura!" << std::endl;
        return -1;
    }

    // Setare cheie publica certificat
    if (X509_set_pubkey(newcert, req_pubkey) != 1)
    {
        std::cout << "Eroare setare cheie publica certificat!" << std::endl;
        return -1;
    }

    // Setare valabilitate 365 de zile
    if (!(X509_gmtime_adj(X509_get_notBefore(newcert), 0)))
    {
        std::cout << "Eroare setare start date!" << std::endl;
        return -1;
    }

    if (!(X509_gmtime_adj(X509_get_notAfter(newcert), 31536000L)))
    {
        std::cout << "Eroare setare expiration date!" << std::endl;
        return -1;
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
        return -1;
    }

    // Salvare certificat
    BIO *output = BIO_new_file((const char *)"/home/razvan/certificates/cert.pem", "wb");

    ret = PEM_write_bio_X509(output, newcert);
    if (ret == 0)
    {
        std::cout << "Eroare la scriere in fisier" << std::endl;
        return -1;
    }

    BIO_free_all(output);
    X509_REQ_free(x509_req);
    X509_free(newcert);

    return 0;
}
