#!/bin/bash

g++ SchnorSig.cpp -fPIC -o schnorrlib.dll -shared -lssl -lcrypto
