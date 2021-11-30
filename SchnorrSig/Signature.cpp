#include "Utils.cpp"

typedef struct schnorr_signature
{
    BIGNUM *R;
    BIGNUM *s;
} schnorr_signature;

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

        // Getting the order of the curve
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

        // Generate random integer k between [1,order]
        k = BN_new();
        if (k == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }
        BN_rand_range(k, order);
        //*************************************************************************************************

        // BASE POINT G
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

        // Calculate Q = k * G
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

        // Get xQ
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

        // Hash ( M || xQ)
        unsigned char *hash = new unsigned char[SHA256_DIGEST_LENGTH];
        SHA256(temp, message_length + BN_num_bytes(xQ), hash);
        delete[](temp);
        delete[](xQ_OS);
        //*************************************************************************************************

        // Calculate r = Hash(M || xQ)
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

        // Apoi s = (k - r * private_key) mod n
        // Output r,s
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

        // Getting the order of the curve
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

        // Check if is in the normal bounds
        BIGNUM *test = BN_new();
        BN_one(test);
        if ((BN_cmp(sig.s, order) == 1) || (BN_cmp(sig.s, test) == -1))
        {
            std::cout << "S component of the signature is out of bounds";
            throw VERIFICATION_ERROR;
        }
        BN_free(test);
        //*************************************************************************************************

        // BASE POINT G
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

        // Q = s * G
        Q = EC_POINT_new(group);
        // s * G
        EC_POINT_mul(group, Q, NULL, G, sig.s, NULL);
        const EC_POINT *P = EC_KEY_get0_public_key(key);
        EC_POINT *T = EC_POINT_new(group);

        // r * P
        EC_POINT_mul(group, T, NULL, P, sig.R, NULL);

        // s * G + r * P
        EC_POINT_add(group, Q, Q, T, NULL);

        if (EC_POINT_is_at_infinity(group, Q))
        {
            std::cout << "Verification error" << std::endl;
            throw VERIFICATION_ERROR;
        }

        // get xQ
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

        // Hash ( M || xQ)
        unsigned char *hash = new unsigned char[SHA256_DIGEST_LENGTH];
        SHA256(temp, message_length + BN_num_bytes(xQ), hash);
        //*************************************************************************************************

        // Calculate v = Hash(M || xQ)
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

        // Compare v with r
        // if v == r => verification successful
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

int Schnorr_Multiple_Sign(EC_KEY **keys, int signers_number, const char *message, int message_length, schnorr_signature &sig)
{

    EC_POINT *G, *Q;
    BIGNUM *x, *y, *ks[signers_number], *order, *xQ;
    BIGNUM *k = BN_new();
    BN_zero(k);
    EC_GROUP *group;

    try
    {
        group = EC_GROUP_new_by_curve_name(NID_secp256k1);
        if (group == nullptr)
        {
            std::cout << "The curve group does not exist!" << std::endl;
            throw GROUP_ERROR;
        }

        // Getting the order of the curve
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

        BN_CTX *ctx = BN_CTX_new();
        // Generate random integers k between [1,order]
        for (int i = 0; i < signers_number; i++)
        {
            ks[i] = BN_new();
            if (ks[i] == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }
            BN_rand_range(ks[i], order);
            BN_mod_add(k, k, ks[i], order, ctx);
        }

        //*************************************************************************************************

        // BASE POINT G
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
        // Calculate Q = k * G
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

        // Get xQ
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

        // Hash ( M || xQ)
        unsigned char *hash = new unsigned char[SHA256_DIGEST_LENGTH];
        SHA256(temp, message_length + BN_num_bytes(xQ), hash);
        delete[](temp);
        delete[](xQ_OS);
        //*************************************************************************************************

        // Calculate r = Hash(M || xQ)
        sig.R = BN_new();
        if (sig.R == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }

        BN_bin2bn(hash, SHA256_DIGEST_LENGTH, sig.R);
        BN_mod(sig.R, sig.R, order, ctx);
        delete[](hash);
        //*************************************************************************************************

        // Calculate the aggregate private key
        BIGNUM *private_key = BN_new();
        BN_zero(private_key);
        for (int i = 0; i < signers_number; i++)
        {
            const BIGNUM *temp_key = EC_KEY_get0_private_key(keys[i]);
            BN_mod_add(private_key, private_key, temp_key, order, ctx);
        }

        //*************************************************************************************************
        // Apoi s = (k - r * private_key) mod n
        // Output r,s

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