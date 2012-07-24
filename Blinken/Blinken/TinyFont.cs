﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Blinken
{
    public static class Alphabet
    {
        public static Letter Letters = new Letter(null);
    }

    // ripped from http://www.dafont.com/tiny.font
    public class Letter
    {
        public readonly byte [,] Data;

        public Letter(byte [,] data)
        {
            Data = data;
        }

        public Letter this[char c]
        {
            get
            {
                switch (c)
                {
                    case 'A':
                        return A;
                    case 'B':
                        return B;
                    case 'C':
                        return C;
                    case 'D':
                        return D;
                    case 'E':
                        return E;
                    case 'F':
                        return F;
                    case 'G':
                        return G;
                    case 'H':
                        return H;
                    case 'I':
                        return I;
                    case 'J':
                        return J;
                    case 'K':
                        return K;
                    case 'L':
                        return L;
                    case 'M':
                        return M;
                    case 'N':
                        return N;
                    case 'O':
                        return O;
                    case 'P':
                        return P;
                    case 'Q':
                        return Q;
                    case 'R':
                        return R;
                    case 'S':
                        return S;
                    case 'T':
                        return T;
                    case 'U':
                        return U;
                    case 'V':
                        return V;
                    case 'W':
                        return W;
                    case 'X':
                        return X;
                    case 'Y':
                        return Y;
                    case 'Z':
                        return Z;
                    default:
                        return null;
                }
            }
        }

        public static Letter A = new Letter(new byte [,]
        {
            { 0,1,0 },
            { 1,0,1 },
            { 1,1,1 },
            { 1,0,1 },
        });

        public static Letter B = new Letter(new byte[,]
        {
            { 1,1,0 },
            { 1,1,1 },
            { 1,0,1 },
            { 1,1,0 },
        });

        public static Letter C = new Letter(new byte[,]
        {
            { 0,1,1 },
            { 1,0,0 },
            { 1,0,0 },
            { 0,1,1 },
        });

        public static Letter D = new Letter(new byte[,]
        {
            { 1,1,0 },
            { 1,0,1 },
            { 1,0,1 },
            { 1,1,0 },
        });

        public static Letter E = new Letter(new byte[,]
        {
            { 1,1,1 },
            { 1,1,0 },
            { 1,0,0 },
            { 1,1,1 },
        });

        public static Letter F = new Letter(new byte[,]
        {
            { 1,1,1 },
            { 1,0,0 },
            { 1,1,0 },
            { 1,0,0 },
        });

        public static Letter G = new Letter(new byte[,]
        {
            { 0,1,1 },
            { 1,0,0 },
            { 1,0,1 },
            { 0,1,1 },
        });

        public static Letter H = new Letter(new byte[,]
        {
            { 1,0,1 },
            { 1,0,1 },
            { 1,1,1 },
            { 1,0,1 },
        });

        public static Letter I = new Letter(new byte[,]
        {
            { 1,1,1 },
            { 0,1,0 },
            { 0,1,0 },
            { 1,1,1 },
        });

        public static Letter J = new Letter(new byte[,]
        {
            { 0,0,1 },
            { 0,0,1 },
            { 1,0,1 },
            { 0,1,0 },
        });

        public static Letter K = new Letter(new byte[,]
        {
            { 1,0,0,1 },
            { 1,0,1,0 },
            { 1,1,1,0 },
            { 1,0,0,1 },
        });

        public static Letter L = new Letter(new byte[,]
        {
            { 1,0,0 },
            { 1,0,0 },
            { 1,0,0 },
            { 1,1,1 },
        });

        public static Letter M = new Letter(new byte[,]
        {
            { 1,0,0,0,1 },
            { 1,1,0,1,1 },
            { 1,0,1,0,1 },
            { 1,0,0,0,1 },
        });

        public static Letter N = new Letter(new byte[,]
        {
            { 1,0,0,1 },
            { 1,1,0,1 },
            { 1,0,1,1 },
            { 1,0,0,1 },
        });

        public static Letter O = new Letter(new byte[,]
        {
            { 0,1,1,0 },
            { 1,0,0,1 },
            { 1,0,0,1 },
            { 0,1,1,0 },
        });

        public static Letter P = new Letter(new byte[,]
        {
            { 1,1,0 },
            { 1,0,1 },
            { 1,1,0 },
            { 1,0,0 },
        });

        public static Letter Q = new Letter(new byte[,]
        {
            { 0,1,1,0 },
            { 1,0,0,1 },
            { 1,0,0,1 },
            { 0,1,1,0 },
            { 0,0,1,0 },
        });

        public static Letter R = new Letter(new byte[,]
        {
            { 1,1,0 },
            { 1,0,1 },
            { 1,1,0 },
            { 1,0,1 },
        });

        public static Letter S = new Letter(new byte[,]
        {
            { 0,1,1 },
            { 1,0,0 },
            { 0,0,1 },
            { 1,1,0 },
        });

        public static Letter T = new Letter(new byte[,]
        {
            { 1,1,1 },
            { 0,1,0 },
            { 0,1,0 },
            { 0,1,0 },
        });

        public static Letter U = new Letter(new byte[,]
        {
            { 1,0,0,1 },
            { 1,0,0,1 },
            { 1,0,0,1 },
            { 0,1,1,0 },
        });

        public static Letter V = new Letter(new byte[,]
        {
            { 1,0,0,0,1 },
            { 1,0,0,0,1 },
            { 0,1,0,1,0 },
            { 0,0,1,0,0 },
        });

        public static Letter W = new Letter(new byte[,]
        {
            { 1,0,0,0,1 },
            { 1,0,1,0,1 },
            { 1,0,1,0,1 },
            { 0,1,0,1,0 },
        });

        public static Letter X = new Letter(new byte[,]
        {
            { 1,0,1 },
            { 0,1,0 },
            { 1,0,1 },
            { 1,0,1 },
        });

        public static Letter Y = new Letter(new byte[,]
        {
            { 1,0,0,0,1 },
            { 0,1,0,1,0 },
            { 0,0,1,0,0 },
            { 0,0,1,0,0 },
        });

        public static Letter Z = new Letter(new byte[,]
        {
            { 1,1,1,1 },
            { 0,0,1,0 },
            { 0,1,0,0 },
            { 1,1,1,1 },
        });
    }
}
