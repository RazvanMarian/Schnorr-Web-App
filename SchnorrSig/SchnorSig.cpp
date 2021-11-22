#include <stdio.h>
#include <string.h>
#include <iostream>
#include <string>
#include <openssl/ec.h>
#include <openssl/evp.h>
#include <openssl/bn.h>
#include <openssl/sha.h>
#include <openssl/pem.h>

//Base point coordinates
#define xG "79BE667EF9DCBBAC55A06295CE870B07029BFCDB2DCE28D959F2815B16F81798"
#define yG "483ADA7726A3C4655DA4FBFC0E1108A8FD17B448A68554199C47D08FFB10D4B8"

#define BASE_ERROR 100
#define MEMORY_ERROR 101
#define GROUP_ERROR 102
#define POINT_ERROR 104
#define POINT_CONVERSION_ERROR 105
#define CALCULATION_ERROR 106
#define GENERATE_ERROR 107
#define ORDER_ERROR 108
#define SIGNATURE_ERROR 110
#define KEY_ERROR 112
#define VERIFICATION_ERROR 114

extern "C"
{

    typedef struct schnorr_signature
    {
        BIGNUM *R;
        BIGNUM *s;
    };

    int Gen(EC_KEY **key)
    {
        EC_POINT *Q, *G;
        BIGNUM *a, *x, *y, *order;
        EC_GROUP *group;

        try
        {
            group = EC_GROUP_new_by_curve_name(NID_secp256k1);
            if (group == nullptr)
            {
                std::cout << "The curve group does not exist!" << std::endl;
                throw GROUP_ERROR;
            }

            //BASE POINT G
            G = EC_POINT_new(group);
            if (G == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }
            x = BN_new();
            y = BN_new();
            if (x == nullptr || y == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }
            BN_hex2bn(&x, xG);
            BN_hex2bn(&y, yG);

            if (!EC_POINT_set_affine_coordinates(group, G, x, y, nullptr))
            {
                std::cout << "The point does not belong to the curve!" << std::endl;
                throw POINT_ERROR;
            }

            if (!EC_POINT_is_on_curve(group, G, nullptr))
            {
                std::cout << "The point does not belong to the curve!" << std::endl;
                throw POINT_ERROR;
            }

            //*************************************************************************************************************
            order = BN_new();
            if (EC_GROUP_get_order(group, order, nullptr) == 0)
            {
                std::cout << "Order error" << std::endl;
                throw ORDER_ERROR;
            }

            a = BN_new();
            if (a == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }

            Q = EC_POINT_new(group);
            if (Q == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }

            //Generate random integer a between [1,order]
            BN_rand_range(a, order);
            //Calculate Q = a * P
            EC_POINT_mul(group, Q, nullptr, G, a, nullptr);

            if (!EC_POINT_is_on_curve(group, Q, nullptr))
            {
                std::cout << "The point does not belong to the curve!" << std::endl;
                throw POINT_ERROR;
            }

            *key = EC_KEY_new();
            if (!EC_KEY_set_group(*key, group))
            {
                std::cout << "Key generation error!" << std::endl;
                throw KEY_ERROR;
            }

            if (!EC_KEY_set_private_key(*key, a))
            {
                std::cout << "Key generation error!" << std::endl;
                throw KEY_ERROR;
            }

            if (!EC_KEY_set_public_key(*key, Q))
            {
                std::cout << "Key generation error!" << std::endl;
                throw KEY_ERROR;
            }
        }
        catch (int err)
        {
            std::cout << "ERROR: " << err << std::endl;
            return err;
        }

        EC_POINT_free(G);
        EC_POINT_free(Q);
        EC_GROUP_free(group);
        BN_free(a);
        BN_free(x);
        BN_free(y);
        BN_free(order);

        return 0;
    }

    int Schnorr_Sign(EC_KEY *key, const char *message, int message_length, schnorr_signature &sig)
    {
        EC_POINT *G, *Q;
        BIGNUM *x, *y, *k, *order, *xQ;
        EC_GROUP *group;

        try
        {
            group = EC_GROUP_new_by_curve_name(NID_secp256k1);
            if (group == nullptr)
            {
                std::cout << "The curve group does not exist!" << std::endl;
                throw GROUP_ERROR;
            }

            //Getting the order of the curve
            order = BN_new();
            if (order == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }
            if (EC_GROUP_get_order(group, order, nullptr) == 0)
            {
                std::cout << "Order error" << std::endl;
                throw ORDER_ERROR;
            }
            //*************************************************************************************************

            //Generate random integer k between [1,order]
            k = BN_new();
            if (k == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }
            BN_rand_range(k, order);
            //*************************************************************************************************

            //BASE POINT G
            G = EC_POINT_new(group);
            if (G == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }
            x = BN_new();
            y = BN_new();
            if (x == nullptr || y == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }
            BN_hex2bn(&x, xG);
            BN_hex2bn(&y, yG);

            if (!EC_POINT_set_affine_coordinates(group, G, x, y, nullptr))
            {
                std::cout << "The point does not belong to the curve!" << std::endl;
                throw POINT_ERROR;
            }
            //*************************************************************************************************

            //Calculate Q = k * G
            Q = EC_POINT_new(group);
            if (Q == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }
            EC_POINT_mul(group, Q, NULL, G, k, NULL);
            if (!EC_POINT_is_on_curve(group, Q, NULL))
            {
                std::cout << "The point does not belong to the curve!" << std::endl;
                throw POINT_ERROR;
            }
            //*************************************************************************************************

            //Get xQ
            xQ = BN_new();
            if (xQ == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }
            EC_POINT_get_affine_coordinates(group, Q, xQ, NULL, NULL);
            printf("\n");

            unsigned char *xQ_OS = new unsigned char[BN_num_bytes(xQ)];
            BN_bn2bin(xQ, (unsigned char *)xQ_OS);
            //*************************************************************************************************

            // M || xQ
            unsigned char *temp = new unsigned char[message_length + BN_num_bytes(xQ)];
            memcpy(temp, message, message_length);
            memcpy(temp + message_length, xQ_OS, BN_num_bytes(xQ));
            //*************************************************************************************************

            //Hash ( M || xQ)
            unsigned char *hash = new unsigned char[SHA256_DIGEST_LENGTH];
            SHA256(temp, message_length + BN_num_bytes(xQ), hash);
            delete[](temp);
            delete[](xQ_OS);
            //*************************************************************************************************

            //Calculate r = Hash(M || xQ)
            sig.R = BN_new();
            if (sig.R == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }

            BN_bin2bn(hash, SHA256_DIGEST_LENGTH, sig.R);
            BN_CTX *ctx = BN_CTX_new();
            BN_mod(sig.R, sig.R, order, ctx);
            delete[](hash);
            //*************************************************************************************************

            //Apoi s = (k - r * private_key) mod n
            //Output r,s
            const BIGNUM *private_key = BN_new();
            private_key = EC_KEY_get0_private_key(key);

            BIGNUM *temporary = BN_new();
            sig.s = BN_new();

            BN_mod_mul(temporary, sig.R, private_key, order, ctx);
            BN_mod_sub(sig.s, k, temporary, order, ctx);
        }
        catch (int err)
        {
            std::cout << "ERROR: " << err << std::endl;
            return err;
        }

        EC_POINT_free(G);
        EC_POINT_free(Q);
        BN_free(order);
        BN_free(x);
        BN_free(y);
        BN_free(k);
        BN_free(xQ);
        EC_GROUP_free(group);
        return 0;
    }

    int Verify_Sign(EC_KEY *key, const char *message, int message_length, schnorr_signature &sig)
    {
        BIGNUM *order, *x, *y;
        EC_POINT *G, *Q;
        EC_GROUP *group;
        try
        {
            group = EC_GROUP_new_by_curve_name(NID_secp256k1);
            if (group == nullptr)
            {
                std::cout << "The curve group does not exist!" << std::endl;
                throw GROUP_ERROR;
            }

            //Getting the order of the curve
            order = BN_new();
            if (order == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }
            if (EC_GROUP_get_order(group, order, nullptr) == 0)
            {
                std::cout << "Order error" << std::endl;
                throw ORDER_ERROR;
            }
            //*************************************************************************************************

            //Check if is in the normal bounds
            BIGNUM *test = BN_new();
            BN_one(test);
            if ((BN_cmp(sig.s, order) == 1) || (BN_cmp(sig.s, test) == -1))
            {
                std::cout << "S component of the signature is out of bounds";
                throw VERIFICATION_ERROR;
            }
            BN_free(test);
            //*************************************************************************************************

            //BASE POINT G
            G = EC_POINT_new(group);
            if (G == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }
            x = BN_new();
            y = BN_new();
            if (x == nullptr || y == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }
            BN_hex2bn(&x, xG);
            BN_hex2bn(&y, yG);

            if (!EC_POINT_set_affine_coordinates(group, G, x, y, nullptr))
            {
                std::cout << "The point does not belong to the curve!" << std::endl;
                throw POINT_ERROR;
            }
            //*************************************************************************************************

            //Q = s * G
            Q = EC_POINT_new(group);
            //s * G
            EC_POINT_mul(group, Q, NULL, G, sig.s, NULL);
            const EC_POINT *P = EC_KEY_get0_public_key(key);
            EC_POINT *T = EC_POINT_new(group);

            //r * P
            EC_POINT_mul(group, T, NULL, P, sig.R, NULL);

            //s * G + r * P
            EC_POINT_add(group, Q, Q, T, NULL);

            if (EC_POINT_is_at_infinity(group, Q))
            {
                std::cout << "Verification error" << std::endl;
                throw VERIFICATION_ERROR;
            }

            //get xQ
            BIGNUM *xQ = BN_new();
            if (xQ == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }
            EC_POINT_get_affine_coordinates(group, Q, xQ, NULL, NULL);
            printf("\n");

            unsigned char *xQ_OS = new unsigned char[BN_num_bytes(xQ)];
            BN_bn2bin(xQ, (unsigned char *)xQ_OS);
            //*************************************************************************************************

            // M || xQ
            unsigned char *temp = new unsigned char[message_length + BN_num_bytes(xQ)];
            memcpy(temp, message, message_length);
            memcpy(temp + message_length, xQ_OS, BN_num_bytes(xQ));
            //*************************************************************************************************

            //Hash ( M || xQ)
            unsigned char *hash = new unsigned char[SHA256_DIGEST_LENGTH];
            SHA256(temp, message_length + BN_num_bytes(xQ), hash);
            //*************************************************************************************************

            //Calculate v = Hash(M || xQ)
            BIGNUM *v = BN_new();
            if (v == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }

            BN_bin2bn(hash, SHA256_DIGEST_LENGTH, v);
            delete[](hash);
            delete[](xQ_OS);
            BN_free(xQ);

            BN_CTX *ctx = BN_CTX_new();
            BN_mod(v, v, order, ctx);
            BN_CTX_free(ctx);
            //*************************************************************************************************

            //Compare v with r
            //if v == r => verification successful
            if (BN_cmp(v, sig.R) == 0)
            {
                std::cout << "VERIFICATION OK" << std::endl;
            }
            else
            {
                std::cout << "Verification error" << std::endl;
                throw VERIFICATION_ERROR;
            }
            BN_free(v);
        }
        catch (int err)
        {
            std::cout << "ERROR: " << err << std::endl;
            return err;
        }

        EC_POINT_free(G);
        EC_POINT_free(Q);
        BN_free(order);
        BN_free(x);
        BN_free(y);
        EC_GROUP_free(group);
        return 0;
    }

    int PEM_Write_SchnorrPUBKEY(EC_KEY *key, const char *filename)
    {
        if (key == NULL)
        {
            std::cout << "Key is NULL!" << std::endl;
            return -1;
        }

        FILE *fout = fopen(filename, "wb");
        if (fout == NULL)
        {
            std::cout << "The file does not exist!" << std::endl;
            return -1;
        }

        if (!PEM_write_EC_PUBKEY(fout, key))
        {
            std::cout << "Error at writing into the file!" << std::endl;
            return -1;
        }

        return 0;
    }

    int PEM_Write_SchnorrPrivateKEY(EC_KEY *key, const char *filename, const EVP_CIPHER *cipher, unsigned char *kstr, int klen, pem_password_cb *cb, void *u)
    {
        if (key == NULL)
        {
            std::cout << "Key is NULL!" << std::endl;
            return -1;
        }

        FILE *fout = fopen(filename, "wb");
        if (fout == NULL)
        {
            std::cout << "The file does not exist!" << std::endl;
            return -1;
        }

        if (!PEM_write_ECPrivateKey(fout, key, cipher, kstr, klen, cb, u))
        {
            std::cout << "Error at writing into the file!" << std::endl;
            return -1;
        }
    }

    int PEM_Read_SchnorrPUBKEY(EC_KEY **key, const char *filename, pem_password_cb *cb, void *u)
    {
        *key = EC_KEY_new();

        FILE *fin = fopen(filename, "rb");
        if (fin == NULL)
        {
            std::cout << "The file does not exist!" << std::endl;
            return -1;
        }

        PEM_read_EC_PUBKEY(fin, key, cb, u);
        if (*key == NULL)
        {
            std::cout << "Error reading the key!" << std::endl;
            return -1;
        }

        return 0;
    }

    int PEM_Read_SchnorrPrivateKEY(EC_KEY **key, const char *filename, pem_password_cb *cb, void *u)
    {
        *key = EC_KEY_new();

        FILE *fin = fopen(filename, "rb");
        if (fin == NULL)
        {
            std::cout << "The file does not exist!" << std::endl;
            return -1;
        }

        PEM_read_ECPrivateKey(fin, key, cb, u);
        if (*key == NULL)
        {
            std::cout << "Error reading the key!" << std::endl;
            return -1;
        }

        return 0;
    }

    void Hello(const char *strFrom)
    {

        BIGNUM *a = BN_new();
        BN_rand(a, 1024, BN_RAND_BOTTOM_ANY, BN_RAND_TOP_ANY);
        char *to = (char *)malloc(BN_num_bytes(a));
        to = BN_bn2hex(a);
        std::string str((char *)to);
        std::cout << str << std::endl;
        std::cout << "Ceva" << std::endl;
        std::string anotherstr = strFrom;
        std::cout << anotherstr << std::endl;
    }

    void test_sign(const char *hash)
    {
        std::cout << "Hash:" << hash << std::endl;

        EC_KEY *key;
        int res = Gen(&key);
        if (res != 0)
        {
            std::cout << "Eroare la generarea cheii" << std::endl;
            return;
        }

        schnorr_signature sig;
        res = Schnorr_Sign(key, hash, SHA256_DIGEST_LENGTH, sig);
        if (res != 0)
        {
            std::cout << "Eroare la generarea crearea semnaturii" << std::endl;
            return;
        }
        BN_print_fp(stdout, sig.s);
        std::cout << std::endl;
        res = Verify_Sign(key, hash, SHA256_DIGEST_LENGTH, sig);
        if (res != 0)
        {
            std::cout << "Eroare la verificare semnatura";
            return;
        }

        std::cout << "Generare, Semnare, Verificare OK :) phew" << std::endl;
    }
}