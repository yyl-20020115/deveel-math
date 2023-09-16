using System;

using Xunit;

namespace Deveel.Math;

public class BigIntegerTest
{
    public BigIntegerTest()
    {
        TwoToTheSeventy = BigMath.Pow(Two, 70);
        SetUp();
    }

    private readonly BigInteger MinusTwo = BigInteger.Parse("-2", 10);

    private readonly BigInteger MinusOne = BigInteger.Parse("-1", 10);

    private readonly BigInteger Zero = BigInteger.Parse("0", 10);

    private readonly BigInteger One = BigInteger.Parse("1", 10);

    private readonly BigInteger Two = BigInteger.Parse("2", 10);

    private readonly BigInteger Ten = BigInteger.Parse("10", 10);

    private readonly BigInteger Sixteen = BigInteger.Parse("16", 10);

    public BigIntegerTest(BigInteger sixteen)
    {
        this.Sixteen = sixteen;
    }

    private readonly BigInteger OneThousand = BigInteger.Parse("1000", 10);

    private readonly BigInteger AZillion = BigInteger.Parse("100000000000000000000000000000000000000000000000000", 10);

    private readonly BigInteger TwoToTheTen = BigInteger.Parse("1024", 10);

    private readonly BigInteger TwoToTheSeventy;

    private Random Rand = new();

    private BigInteger bi;

    private BigInteger bi1;

    private BigInteger bi2;

    private BigInteger bi3;

    private BigInteger bi11;

    private BigInteger bi22;

    private BigInteger bi33;

    private BigInteger bi12;

    private BigInteger bi23;

    private BigInteger bi13;

    private BigInteger largePos;

    private BigInteger smallPos;

    private BigInteger largeNeg;

    private BigInteger smallNeg;

    private BigInteger[][] booleanPairs;

    private void SetUp()
    {
        bi1 = BigInteger.Parse("2436798324768978", 16);
        bi2 = BigInteger.Parse("4576829475724387584378543764555", 16);
        bi3 = BigInteger.Parse("43987298363278574365732645872643587624387563245", 16);

        bi33 = BigInteger.Parse(
                "10730846694701319120609898625733976090865327544790136667944805934175543888691400559249041094474885347922769807001",
                10);
        bi22 = BigInteger.Parse(
                "33301606932171509517158059487795669025817912852219962782230629632224456249",
                10);
        bi11 = BigInteger.Parse("6809003003832961306048761258711296064", 10);
        bi23 = BigInteger.Parse(
                "597791300268191573513888045771594235932809890963138840086083595706565695943160293610527214057",
                10);
        bi13 = BigInteger.Parse(
                "270307912162948508387666703213038600031041043966215279482940731158968434008",
                10);
        bi12 = BigInteger.Parse(
                "15058244971895641717453176477697767050482947161656458456", 10);

        largePos = BigInteger.Parse(
                "834759814379857314986743298675687569845986736578576375675678998612743867438632986243982098437620983476924376",
                16);
        smallPos = BigInteger.Parse("48753269875973284765874598630960986276", 16);
        largeNeg = BigInteger.Parse(
                "-878824397432651481891353247987891423768534321387864361143548364457698487264387568743568743265873246576467643756437657436587436",
                16);
        smallNeg = BigInteger.Parse("-567863254343798609857456273458769843", 16);
        booleanPairs = new BigInteger[4][];
        booleanPairs[0] = new BigInteger[] { largePos, smallPos };
        booleanPairs[1] = new BigInteger[] { largePos, smallNeg };
        booleanPairs[2] = new BigInteger[] { largeNeg, smallPos };
        booleanPairs[3] = new BigInteger[] { largeNeg, smallNeg };
        /*
		booleanPairs = new BigInteger[][] { { largePos, smallPos },
			{ largePos, smallNeg }, { largeNeg, smallPos },
			{ largeNeg, smallNeg } };
		*/
    }

    [Fact]
    public void ConstructorIRandom()
    {
        // regression test for HARMONY-1047
        Assert.Throws<OverflowException>(() => new BigInteger(Int32.MaxValue, (Random)null));

        bi = new BigInteger(70, Rand);
        bi2 = new BigInteger(70, Rand);
        Assert.True(bi.CompareTo(Zero) >= 0, "Random number is negative");
        Assert.True(bi.CompareTo(TwoToTheSeventy) < 0, "Random number is too big");
        Assert.True(!bi.Equals(bi2), "Two random numbers in a row are the same (might not be a bug but it very likely is)");
        Assert.True(new BigInteger(0, Rand).Equals(BigInteger.Zero), "Not zero");
    }

    [Fact]
    public void CostructorIIRandom()
    {
        bi = new BigInteger(10, 5, Rand);
        bi2 = new BigInteger(10, 5, Rand);
        Assert.True(bi.CompareTo(Zero) >= 0, "Random number one is negative");
        Assert.True(bi.CompareTo(TwoToTheTen) < 0, "Random number one is too big");
        Assert.True(bi2.CompareTo(Zero) >= 0, "Random number two is negative");
        Assert.True(bi2.CompareTo(TwoToTheTen) < 0, "Random number two is too big");

        Random rand_b = new();
        BigInteger bi_b;
        int[] certainty = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, Int32.MinValue, Int32.MinValue + 1, -2, -1 };
        for (int i = 2; i <= 20; i++)
        {
            for (int c = 0; c < certainty.Length; c++)
            {
                bi_b = new BigInteger(i, c, rand_b); // Create BigInteger
                Assert.True(bi_b.BitLength == i, "Bit length incorrect");
            }
        }
    }

    [Fact]
    public void ConstructorBytes()
    {
        var myByteArray = new byte[] { (byte)0x00, (byte)0xFF, (byte)0xFE };
        bi = new BigInteger(myByteArray);
        Assert.True(bi.Equals(BigInteger.SetBit(BigInteger.Zero, 16) - Two), "Incorrect value for pos number");
        myByteArray = new byte[] { (byte)0xFF, (byte)0xFE };
        bi = new BigInteger(myByteArray);
        Assert.True(bi.Equals(MinusTwo), "Incorrect value for neg number");
    }

    [Fact]
    public void ConstructorIBytes()
    {
        var myByteArray = new byte[] { (byte)0xFF, (byte)0xFE };
        bi = new BigInteger(1, myByteArray);
        Assert.True(bi.Equals(BigInteger.SetBit(BigInteger.Zero, 16) - Two), "Incorrect value for pos number");
        bi = new BigInteger(-1, myByteArray);
        Assert.True(bi.Equals(-(BigInteger.SetBit(BigInteger.Zero, 16) - Two)), "Incorrect value for neg number");
        myByteArray = new byte[] { (byte)0, (byte)0 };
        bi = new BigInteger(0, myByteArray);
        Assert.True(bi.Equals(Zero), "Incorrect value for zero");
        myByteArray = new byte[] { (byte)1 };

        Assert.Throws<FormatException>(() => new BigInteger(0, myByteArray));
    }

    [Fact]
    public void ParseStringEmpty()
    {
        Assert.Throws<FormatException>(() => BigInteger.Parse(""));
    }

    [Fact]
    public void ToByteArray()
    {
        var myByteArray = new byte[] { 97, 33, 120, 124, 50, 2, 0, 0, 0, 12, 124, 42 };
        var anotherByteArray = new BigInteger(myByteArray).ToByteArray();
        Assert.True(myByteArray.Length == anotherByteArray.Length, "Incorrect byte array returned");
        for (int counter = myByteArray.Length - 1; counter >= 0; counter--)
        {
            Assert.True(myByteArray[counter] == anotherByteArray[counter], "Incorrect values in returned byte array");
        }
    }

    [Fact]
    public void IsProbablePrimeI()
    {
        int fails = 0;
        bi = new BigInteger(20, 20, Rand);
        if (!BigInteger.IsProbablePrime(bi, 17))
        {
            fails++;
        }
        bi = BigInteger.Parse("4", 10);
        if (BigInteger.IsProbablePrime(bi, 17))
        {
            throw new Exception("IsProbablePrime failed for: " + bi.ToString());
        }
        bi = BigInteger.FromInt64(17L * 13L);
        if (BigInteger.IsProbablePrime(bi, 17))
        {
            throw new Exception("IsProbablePrime failed for: " + bi.ToString());
        }
        for (long a = 2; a < 1000; a++)
        {
            if (IsPrime(a))
            {
                Assert.True(BigInteger.IsProbablePrime(BigInteger.FromInt64(a), 5), "false negative on prime number <1000");
            }
            else if (BigInteger.IsProbablePrime(BigInteger.FromInt64(a), 17))
            {
#if !PORTABLE
                Console.Out.WriteLine("IsProbablePrime failed for: " + a);
#endif
                fails++;
            }
        }
        for (int a = 0; a < 1000; a++)
        {
            bi = (BigInteger.FromInt64(Rand.Next(1000000)) * BigInteger.FromInt64(Rand.Next(1000000)));
            if (BigInteger.IsProbablePrime(bi, 17))
            {
#if !PORTABLE
                Console.Out.WriteLine("IsProbablePrime failed for: " + bi.ToString());
#endif
                fails++;
            }
        }
        for (int a = 0; a < 200; a++)
        {
            bi = new BigInteger(70, Rand) * new BigInteger(70, Rand);
            if (BigInteger.IsProbablePrime(bi, 17))
            {
#if !PORTABLE
                Console.Out.WriteLine("IsProbablePrime failed for: " + bi.ToString());
#endif
                fails++;
            }
        }

        Assert.True(fails <= 1, "Too many false positives - may indicate a problem");
    }

    [Fact]
    public void EqualsObject()
    {
        Assert.True(Zero.Equals(BigInteger.FromInt64(0)), "0=0");
        Assert.True(BigInteger.FromInt64(-123).Equals(BigInteger.FromInt64(-123)), "-123=-123");
        Assert.True(!Zero.Equals(One), "0=1");
        Assert.True(!Zero.Equals(MinusOne), "0=-1");
        Assert.True(!One.Equals(MinusOne), "1=-1");
        Assert.True(bi3.Equals(bi3), "bi3=bi3");
        Assert.True(bi3.Equals(-(-bi3)), "bi3=copy of bi3");
        Assert.True(!bi3.Equals(bi2), "bi3=bi2");
    }

    [Fact]
    public void CompareToBigInteger()
    {
        Assert.True(One.CompareTo(Two) < 0, "Smaller number returned >= 0");
        Assert.True(Two.CompareTo(One) > 0, "Larger number returned >= 0");
        Assert.True(One.CompareTo(One) == 0, "Equal numbers did not return 0");
        Assert.True((-Two).CompareTo(One) < 0, "Neg number messed things up");
    }

    [Fact]
    public void CompareToBigInteger_Op()
    {
        Assert.True(One < Two, "Smaller number returned >= 0");
        Assert.True(Two > 0, "Larger number returned >= 0");
        Assert.True(One == One, "Equal numbers did not return 0");
        Assert.True(-Two < 0, "Neg number messed things up");
    }


    [Fact]
    public void ToInt32()
    {
        Assert.True(TwoToTheSeventy.ToInt32() == 0, "Incorrect ToInt32 for 2**70");
        Assert.True(Two.ToInt32() == 2, "Incorrect ToInt32 for 2");
    }

    [Fact]
    public void ToInt64()
    {
        Assert.True(TwoToTheSeventy.ToInt64() == 0, "Incorrect ToInt64 for 2**70");
        Assert.True(Two.ToInt64() == 2, "Incorrect ToInt64 for 2");
    }

    [Fact]
    public void ValueOfJ()
    {
        Assert.True(BigInteger.FromInt64(2L).Equals(Two), "Incurred number returned for 2");
        Assert.True(BigInteger.FromInt64(200L).Equals(BigInteger.FromInt64(139) + (BigInteger.FromInt64(61))),
                      "Incurred number returned for 200");
    }

    [Fact]
    public void AddBigInteger()
    {
        Assert.True((AZillion + AZillion + (-AZillion)).Equals(AZillion), "Incorrect sum--wanted a zillion");
        Assert.True((Zero + Zero).Equals(Zero), "0+0");
        Assert.True((Zero + One).Equals(One), "0+1");
        Assert.True((One + Zero).Equals(One), "1+0");
        Assert.True((One + One).Equals(Two), "1+1");
        Assert.True((Zero + MinusOne).Equals(MinusOne), "0+(-1)");
        Assert.True((MinusOne + Zero).Equals(MinusOne), "(-1)+0");
        Assert.True((MinusOne + MinusOne).Equals(MinusTwo), "(-1)+(-1)");
        Assert.True((One + MinusOne).Equals(Zero), "1+(-1)");
        Assert.True((MinusOne + One).Equals(Zero), "(-1)+1");

        for (int i = 0; i < 200; i++)
        {
            BigInteger midbit = BigInteger.SetBit(Zero, i);
            Assert.True((midbit + midbit).Equals(BigInteger.SetBit(Zero, i + 1)), "add fails to carry on bit " + i);
        }

        BigInteger bi2p3 = bi2 + bi3;
        BigInteger bi3p2 = bi3 + bi2;
        Assert.True(bi2p3.Equals(bi3p2), "bi2p3=bi3p2");
    }

    [Fact]
    public void Negate()
    {
        Assert.True((-Zero).Equals(Zero), "Single negation of zero did not result in zero");
        Assert.True(!(-AZillion).Equals(AZillion), "Single negation resulted in original nonzero number");
        Assert.True((-(-AZillion)).Equals(AZillion), "Double negation did not result in original number");

        Assert.True((-Zero).Equals(Zero), "0.neg");
        Assert.True((-One).Equals(MinusOne), "1.neg");
        Assert.True((-Two).Equals(MinusTwo), "2.neg");
        Assert.True((-MinusOne).Equals(One), "-1.neg");
        Assert.True((-MinusTwo).Equals(Two), "-2.neg");
        Assert.True(
            unchecked((-BigInteger.FromInt64(0x62EB40FEF85AA9EBL * 2)).Equals(BigInteger.FromInt64(-0x62EB40FEF85AA9EBL * 2))),
            "0x62EB40FEF85AA9EBL*2.neg");
        for (int i = 0; i < 200; i++)
        {
            BigInteger midbit = BigInteger.SetBit(Zero, i);
            BigInteger negate = -midbit;
            Assert.True((-negate).Equals(midbit), "negate negate");
            Assert.True(((-midbit) + midbit).Equals(Zero), "neg fails on bit " + i);
        }
    }

    [Fact]
    public void Signum()
    {
        Assert.True(Two.Sign == 1, "Wrong positive signum");
        Assert.True(Zero.Sign == 0, "Wrong zero signum");
        Assert.True((-Zero).Sign == 0, "Wrong neg zero signum");
        Assert.True((-Two).Sign == -1, "Wrong neg signum");
    }

    [Fact]
    public void Abs()
    {
        Assert.True(BigMath.Abs(-AZillion).Equals(BigMath.Abs(AZillion)), "Invalid number returned for zillion");
        Assert.True(BigMath.Abs(-Zero).Equals(Zero), "Invalid number returned for zero neg");
        Assert.True(BigMath.Abs(Zero).Equals(Zero), "Invalid number returned for zero");
        Assert.True(BigMath.Abs(-Two).Equals(Two), "Invalid number returned for two");
    }

    [Fact]
    public void PowI()
    {
        Assert.True(BigMath.Pow(Two, 10).Equals(TwoToTheTen), "Incorrect exponent returned for 2**10");
        Assert.True((BigMath.Pow(Two, 30) * BigMath.Pow(Two, 40)).Equals(TwoToTheSeventy), "Incorrect exponent returned for 2**70");
        Assert.True(BigMath.Pow(Ten, 50).Equals(AZillion), "Incorrect exponent returned for 10**50");
    }

    [Fact]
    public void MmodInverseBigInteger()
    {
        BigInteger a = Zero, mod, inv;
        for (int j = 3; j < 50; j++)
        {
            mod = BigInteger.FromInt64(j);
            for (int i = -j + 1; i < j; i++)
            {
                try
                {
                    a = BigInteger.FromInt64(i);
                    inv = BigMath.ModInverse(a, mod);
                    Assert.True(One.Equals(((a * inv) % mod)), "bad inverse: " + a + " inv mod " + mod + " equals " + inv);
                    Assert.True(inv.CompareTo(mod) < 0, "inverse greater than modulo: " + a + " inv mod " + mod + " equals " + inv);
                    Assert.True(inv.CompareTo(BigInteger.Zero) >= 0, "inverse less than zero: " + a + " inv mod " + mod + " equals " + inv);
                }
                catch (ArithmeticException)
                {
                    Assert.True(!One.Equals(BigMath.Gcd(a, mod)), "should have found inverse for " + a + " mod " + mod);
                }
            }
        }
        for (int j = 1; j < 10; j++)
        {
            mod = bi2 + BigInteger.FromInt64(j);
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    a = bi3 + (BigInteger.FromInt64(i));
                    inv = BigMath.ModInverse(a, mod);
                    Assert.True(One.Equals((a * inv) % mod), "bad inverse: " + a + " inv mod " + mod + " equals " + inv);
                    Assert.True(inv.CompareTo(mod) < 0, "inverse greater than modulo: " + a + " inv mod " + mod + " equals " + inv);
                    Assert.True(inv.CompareTo(BigInteger.Zero) >= 0, "inverse less than zero: " + a + " inv mod " + mod + " equals " + inv);
                }
                catch (ArithmeticException)
                {
                    Assert.True(!One.Equals(BigMath.Gcd(a, mod)), "should have found inverse for " + a + " mod " + mod);
                }
            }
        }
    }

    [Fact]
    public void ShiftRightI()
    {
        Assert.True((BigInteger.FromInt64(1) >> 0).Equals(BigInteger.One), "1 >> 0");
        Assert.True((BigInteger.FromInt64(1) >> 1).Equals(BigInteger.Zero), "1 >> 1");
        Assert.True((BigInteger.FromInt64(1) >> 63).Equals(BigInteger.Zero), "1 >> 63");
        Assert.True((BigInteger.FromInt64(1) >> 64).Equals(BigInteger.Zero), "1 >> 64");
        Assert.True((BigInteger.FromInt64(1) >> 65).Equals(BigInteger.Zero), "1 >> 65");
        Assert.True((BigInteger.FromInt64(1) >> 1000).Equals(BigInteger.Zero), "1 >> 1000");
        Assert.True((BigInteger.FromInt64(-1) >> 0).Equals(MinusOne), "-1 >> 0");
        Assert.True((BigInteger.FromInt64(-1) >> 1).Equals(MinusOne), "-1 >> 1");
        Assert.True((BigInteger.FromInt64(-1) >> 63).Equals(MinusOne), "-1 >> 63");
        Assert.True((BigInteger.FromInt64(-1) >> 64).Equals(MinusOne), "-1 >> 64");
        Assert.True((BigInteger.FromInt64(-1) >> 65).Equals(MinusOne), "-1 >> 65");
        Assert.True((BigInteger.FromInt64(-1) >> 1000).Equals(MinusOne), "-1 >> 1000");

        BigInteger a = BigInteger.One;
        BigInteger c = bi3;
        BigInteger E = -bi3;
        BigInteger e = E;
        for (int i = 0; i < 200; i++)
        {
            BigInteger b = BigInteger.SetBit(BigInteger.Zero, i);
            Assert.True(a.Equals(b), "a==b");
            a <<= 1;
            Assert.True(a.Sign >= 0, "a non-neg");

            BigInteger d = bi3 >> i;
            Assert.True(c.Equals(d), "c==d");
            c >>= 1;
            Assert.True((d / Two).Equals(c), ">>1 == /2");
            Assert.True(c.Sign >= 0, "c non-neg");

            BigInteger f = E >> i;
            Assert.True(e.Equals(f), "e==f");
            e >>= 1;
            Assert.True(((f - One) / Two).Equals(e), ">>1 == /2");
            Assert.True(e.Sign == -1, "e negative");

            Assert.True((b >> i).Equals(One), "b >> i");
            Assert.True((b >> i + 1).Equals(Zero), "b >> i+1");
            Assert.True((b >> i - 1).Equals(Two), "b >> i-1");
        }
    }

    [Fact]
    public void ShiftLeftI()
    {
        Assert.True((One << 0).Equals(One), "1 << 0");
        Assert.True((One << 1).Equals(Two), "1 << 1");
        Assert.True((One << 63).Equals(BigInteger.Parse("8000000000000000", 16)), "1 << 63");
        Assert.True((One << 64).Equals(BigInteger.Parse("10000000000000000", 16)), "1 << 64");
        Assert.True((One << 65).Equals(BigInteger.Parse("20000000000000000", 16)), "1 << 65");
        Assert.True((MinusOne << 0).Equals(MinusOne), "-1 << 0");
        Assert.True((MinusOne << 1).Equals(MinusTwo), "-1 << 1");
        Assert.True((MinusOne << 63).Equals(BigInteger.Parse("-9223372036854775808")), "-1 << 63");
        Assert.True((MinusOne << 64).Equals(BigInteger.Parse("-18446744073709551616")), "-1 << 64");
        Assert.True((MinusOne << 65).Equals(BigInteger.Parse("-36893488147419103232")), "-1 << 65");

        BigInteger a = bi3;
        BigInteger c = MinusOne;
        for (int i = 0; i < 200; i++)
        {
            BigInteger b = bi3 << i;
            Assert.True(a.Equals(b), "a==b");
            Assert.True((a >> i).Equals(bi3), "a >> i == bi3");
            a <<= 1;
            Assert.True((b * Two).Equals(a), "<<1 == *2");
            Assert.True(a.Sign >= 0, "a non-neg");
            Assert.True(a.BitCount == b.BitCount, "a.bitCount==b.bitCount");

            BigInteger d = MinusOne << i;
            Assert.True(c.Equals(d), "c==d");
            c <<= 1;
            Assert.True((d * Two).Equals(c), "<<1 == *2 negative");
            Assert.True(c.Sign == -1, "c negative");
            Assert.True((d >> i).Equals(MinusOne), "d >> i == minusOne");
        }
    }

    [Fact]
    public void MultiplyBigInteger()
    {
        SetUp();
        Assert.True((AZillion + AZillion + AZillion).Equals((AZillion * BigInteger.Parse("3", 10))),
                      "Incorrect sum--wanted three zillion");

        Assert.True((Zero * Zero).Equals(Zero), "0*0");
        Assert.True((Zero * One).Equals(Zero), "0*1");
        Assert.True((One * Zero).Equals(Zero), "1*0");
        Assert.True((One * One).Equals(One), "1*1");
        Assert.True((Zero * MinusOne).Equals(Zero), "0*(-1)");
        Assert.True((MinusOne * Zero).Equals(Zero), "(-1)*0");
        Assert.True((MinusOne * MinusOne).Equals(One), "(-1)*(-1)");
        Assert.True((One * MinusOne).Equals(MinusOne), "1*(-1)");
        Assert.True((MinusOne * One).Equals(MinusOne), "(-1)*1");

        TestAllMults(bi1, bi1, bi11);
        TestAllMults(bi2, bi2, bi22);
        TestAllMults(bi3, bi3, bi33);
        TestAllMults(bi1, bi2, bi12);
        TestAllMults(bi1, bi3, bi13);
        TestAllMults(bi2, bi3, bi23);
    }

    [Fact]
    public void DivideBigInteger()
    {
        TestAllDivs(bi33, bi3);
        TestAllDivs(bi22, bi2);
        TestAllDivs(bi11, bi1);
        TestAllDivs(bi13, bi1);
        TestAllDivs(bi13, bi3);
        TestAllDivs(bi12, bi1);
        TestAllDivs(bi12, bi2);
        TestAllDivs(bi23, bi2);
        TestAllDivs(bi23, bi3);
        TestAllDivs(largePos, bi1);
        TestAllDivs(largePos, bi2);
        TestAllDivs(largePos, bi3);
        TestAllDivs(largeNeg, bi1);
        TestAllDivs(largeNeg, bi2);
        TestAllDivs(largeNeg, bi3);
        TestAllDivs(largeNeg, largePos);
        TestAllDivs(largePos, largeNeg);
        TestAllDivs(bi3, bi3);
        TestAllDivs(bi2, bi2);
        TestAllDivs(bi1, bi1);
        TestDivRanges(bi1);
        TestDivRanges(bi2);
        TestDivRanges(bi3);
        TestDivRanges(smallPos);
        TestDivRanges(largePos);
        TestDivRanges(BigInteger.Parse("62EB40FEF85AA9EB", 16));
        TestAllDivs(BigInteger.FromInt64(0xCC0225953CL), BigInteger
                .FromInt64(0x1B937B765L));

        Assert.Throws<ArithmeticException>(() => largePos / Zero);
        Assert.Throws<ArithmeticException>(() => bi1 / Zero);
        Assert.Throws<ArithmeticException>(() => -bi3 / Zero);
        Assert.Throws<ArithmeticException>(() => Zero / Zero);
    }

    [Fact]
    public void RemainderBigInteger()
    {
        Assert.Throws<ArithmeticException>(() => BigMath.Remainder(largePos, Zero));
        Assert.Throws<ArithmeticException>(() => BigMath.Remainder(bi1, Zero));
        Assert.Throws<ArithmeticException>(() => BigMath.Remainder(-bi3, Zero));
        Assert.Throws<ArithmeticException>(() => BigMath.Remainder(Zero, Zero));
    }

    [Fact]
    public void ModLBigInteger()
    {
        Assert.Throws<ArithmeticException>(() => largePos % Zero);
        Assert.Throws<ArithmeticException>(() => bi1 % Zero);
        Assert.Throws<ArithmeticException>(() => -bi3 % Zero);
        Assert.Throws<ArithmeticException>(() => Zero % Zero);
    }

    [Fact]
    public void DivideAndRemainderBigInteger()
    {
        BigInteger remainder;

        Assert.Throws<ArithmeticException>(() => BigMath.DivideAndRemainder(largePos, Zero, out remainder));
        Assert.Throws<ArithmeticException>(() => BigMath.DivideAndRemainder(bi1, Zero, out remainder));
        Assert.Throws<ArithmeticException>(() => BigMath.DivideAndRemainder(-bi3, Zero, out remainder));
        Assert.Throws<ArithmeticException>(() => BigMath.DivideAndRemainder(Zero, Zero, out remainder));
    }

    [Fact]
    public void ParseString()
    {
        Assert.True(BigInteger.Parse("0").Equals(BigInteger.FromInt64(0)), "new(0)");
        Assert.True(BigInteger.Parse("1").Equals(BigInteger.FromInt64(1)), "new(1)");
        Assert.True(BigInteger.Parse("12345678901234").Equals(BigInteger.FromInt64(12345678901234L)), "new(12345678901234)");
        Assert.True(BigInteger.Parse("-1").Equals(BigInteger.FromInt64(-1)), "new(-1)");
        Assert.True(BigInteger.Parse("-12345678901234").Equals(BigInteger.FromInt64(-12345678901234L)), "new(-12345678901234)");
    }

    [Fact]
    public void ParseStringI()
    {
        Assert.True(BigInteger.Parse("0", 16).Equals(BigInteger.FromInt64(0)), "new(0,16)");
        Assert.True(BigInteger.Parse("1", 16).Equals(BigInteger.FromInt64(1)), "new(1,16)");
        Assert.True(BigInteger.Parse("ABF345678901234", 16).Equals(BigInteger.FromInt64(0xABF345678901234L)), "new(ABF345678901234,16)");
        Assert.True(BigInteger.Parse("abf345678901234", 16).Equals(BigInteger.FromInt64(0xABF345678901234L)), "new(abf345678901234,16)");
        Assert.True(BigInteger.Parse("-1", 16).Equals(BigInteger.FromInt64(-1)), "new(-1,16)");
        Assert.True(BigInteger.Parse("-ABF345678901234", 16).Equals(BigInteger.FromInt64(-0xABF345678901234L)), "new(-ABF345678901234,16)");
        Assert.True(BigInteger.Parse("-abf345678901234", 16).Equals(BigInteger.FromInt64(-0xABF345678901234L)), "new(-abf345678901234,16)");
        Assert.True(BigInteger.Parse("-101010101", 2).Equals(BigInteger.FromInt64(-341)), "new(-101010101,2)");
    }

    [Fact]
    public void TestToString()
    {
        Assert.True("0".Equals(BigInteger.FromInt64(0).ToString()), "0.ToString");
        Assert.True("1".Equals(BigInteger.FromInt64(1).ToString()), "1.ToString");
        Assert.True("12345678901234".Equals(BigInteger.FromInt64(12345678901234L).ToString()), "12345678901234.ToString");
        Assert.True("-1".Equals(BigInteger.FromInt64(-1).ToString()), "-1.ToString");
        Assert.True("-12345678901234".Equals(BigInteger.FromInt64(-12345678901234L).ToString()), "-12345678901234.ToString");
    }

    [Fact]
    public void ToStringI()
    {
        Assert.True("0".Equals(BigInteger.FromInt64(0).ToString(16)), "0.ToString(16)");
        Assert.True("1".Equals(BigInteger.FromInt64(1).ToString(16)), "1.ToString(16)");
        Assert.True("abf345678901234".Equals(BigInteger.FromInt64(0xABF345678901234L).ToString(16)), "ABF345678901234.ToString(16)");
        Assert.True("-1".Equals(BigInteger.FromInt64(-1).ToString(16)), "-1.ToString(16)");
        Assert.True("-abf345678901234".Equals(BigInteger.FromInt64(-0xABF345678901234L).ToString(16)), "-ABF345678901234.ToString(16)");
        Assert.True("-101010101".Equals(BigInteger.FromInt64(-341).ToString(2)), "-101010101.ToString(2)");
    }

    [Fact]
    public void AndLBigInteger()
    {
        foreach (BigInteger[] element in booleanPairs)
        {
            BigInteger i1 = element[0], i2 = element[1];
            BigInteger res = i1 & i2;
            Assert.True(res.Equals(i2 & i1), "symmetry of and");
            int len = System.Math.Max(i1.BitLength, i2.BitLength) + 66;
            for (int i = 0; i < len; i++)
            {
                Assert.True((BigInteger.TestBit(i1, i) && BigInteger.TestBit(i2, i)) == BigInteger.TestBit(res, i), "and");
            }
        }
    }

    [Fact]
    public void OrBigInteger()
    {
        foreach (BigInteger[] element in booleanPairs)
        {
            BigInteger i1 = element[0], i2 = element[1];
            BigInteger res = i1 | i2;
            Assert.True(res.Equals(i2 | i1), "symmetry of or");
            int len = System.Math.Max(i1.BitLength, i2.BitLength) + 66;
            for (int i = 0; i < len; i++)
            {
                Assert.True((BigInteger.TestBit(i1, i) || BigInteger.TestBit(i2, i)) == BigInteger.TestBit(res, i), "or");
            }
        }
    }

    [Fact]
    public void XOrBigInteger()
    {
        foreach (BigInteger[] element in booleanPairs)
        {
            BigInteger i1 = element[0], i2 = element[1];
            BigInteger res = i1 ^ i2;
            Assert.True(res.Equals(i2 ^ i1), "symmetry of xor");
            int len = System.Math.Max(i1.BitLength, i2.BitLength) + 66;
            for (int i = 0; i < len; i++)
            {
                Assert.True((BigInteger.TestBit(i1, i) ^ BigInteger.TestBit(i2, i)) == BigInteger.TestBit(res, i), "xor");
            }
        }
    }

    [Fact]
    public void Not()
    {
        foreach (BigInteger[] element in booleanPairs)
        {
            BigInteger i1 = element[0];
            BigInteger res = ~i1;
            int len = i1.BitLength + 66;
            for (int i = 0; i < len; i++)
            {
                Assert.True(!BigInteger.TestBit(i1, i) == BigInteger.TestBit(res, i), "not");
            }
        }
    }

    [Fact]
    public void AndNotBigInteger()
    {
        foreach (BigInteger[] element in booleanPairs)
        {
            BigInteger i1 = element[0], i2 = element[1];
            BigInteger res = BigMath.AndNot(i1, i2);
            int len = System.Math.Max(i1.BitLength, i2.BitLength) + 66;
            for (int i = 0; i < len; i++)
            {
                Assert.True((BigInteger.TestBit(i1, i) && !BigInteger.TestBit(i2, i)) == BigInteger.TestBit(res, i), "andNot");
            }

            // asymmetrical
            i1 = element[1];
            i2 = element[0];
            res = BigMath.AndNot(i1, i2);
            for (int i = 0; i < len; i++)
            {
                Assert.True((BigInteger.TestBit(i1, i) && !BigInteger.TestBit(i2, i)) == BigInteger.TestBit(res, i), "andNot reversed");
            }
        }

        Assert.Throws<NullReferenceException>(() => BigMath.AndNot(BigInteger.Zero, null));

        BigInteger bi = new(0, new byte[] { });
        Assert.Equal(BigInteger.Zero, BigMath.AndNot(bi, BigInteger.Zero));
    }

    private void TestDiv(BigInteger i1, BigInteger i2)
    {
        BigInteger q = i1 / i2;
        BigInteger r = BigMath.Remainder(i1, i2);
        BigInteger quotient = BigMath.DivideAndRemainder(i1, i2, out BigInteger remainder);

        Assert.True(q.Equals(quotient), "Divide and DivideAndRemainder do not agree");
        Assert.True(r.Equals(remainder), "Remainder and DivideAndRemainder do not agree");
        Assert.True(q.Sign != 0 || q.Equals(Zero), "signum and equals(zero) do not agree on quotient");
        Assert.True(r.Sign != 0 || r.Equals(Zero), "signum and equals(zero) do not agree on remainder");
        Assert.True(q.Sign == 0 || q.Sign == i1.Sign * i2.Sign, "wrong sign on quotient");
        Assert.True(r.Sign == 0 || r.Sign == i1.Sign, "wrong sign on remainder");
        Assert.True(BigMath.Abs(r).CompareTo(BigMath.Abs(i2)) < 0, "remainder out of range");
        Assert.True(((BigMath.Abs(q) + One) * BigMath.Abs(i2)).CompareTo(BigMath.Abs(i1)) > 0, "quotient too small");
        Assert.True((BigMath.Abs(q) * BigMath.Abs(i2)).CompareTo(BigMath.Abs(i1)) <= 0, "quotient too large");
        BigInteger p = q * i2;
        BigInteger a = p + r;
        Assert.True(a.Equals(i1), "(a/b)*b+(a%b) != a");
        try
        {
            BigInteger mod = i1 % i2;
            Assert.True(mod.Sign >= 0, "mod is negative");
            Assert.True(BigMath.Abs(mod).CompareTo(BigMath.Abs(i2)) < 0, "mod out of range");
            Assert.True(r.Sign < 0 || r.Equals(mod), "positive remainder == mod");
            Assert.True(r.Sign >= 0 || r.Equals(mod - i2), "negative remainder == mod - divisor");
        }
        catch (ArithmeticException)
        {
            Assert.True(i2.Sign <= 0, "mod fails on negative divisor only");
        }
    }

    private void TestDivRanges(BigInteger i)
    {
        BigInteger bound = i * Two;
        for (BigInteger j = -bound; j.CompareTo(bound) <= 0; j += i)
        {
            BigInteger innerbound = j + Two;
            BigInteger k = j - Two;
            for (; k.CompareTo(innerbound) <= 0; k += One)
            {
                TestDiv(k, i);
            }
        }
    }

    private static bool IsPrime(long b)
    {
        if (b == 2)
        {
            return true;
        }
        // check for div by 2
        if ((b & 1L) == 0)
        {
            return false;
        }
        long maxlen = ((long)System.Math.Sqrt(b)) + 2;
        for (long x = 3; x < maxlen; x += 2)
        {
            if (b % x == 0)
            {
                return false;
            }
        }
        return true;
    }

    private static void TestAllMults(BigInteger i1, BigInteger i2, BigInteger ans)
    {
        Assert.True((i1 * i2).Equals(ans), "i1*i2=ans");
        Assert.True((i2 * i1).Equals(ans), "i2*i1=ans");
        Assert.True((-i1 * i2).Equals(-ans), "-i1*i2=-ans");
        Assert.True((-i2 * i1).Equals(-ans), "-i2*i1=-ans");
        Assert.True((i1 * -i2).Equals(-ans), "i1*-i2=-ans");
        Assert.True((i2 * -i1).Equals(-ans), "i2*-i1=-ans");
        Assert.True((-i1 * -i2).Equals(ans), "-i1*-i2=ans");
        Assert.True((-i2 * -i1).Equals(ans), "-i2*-i1=ans");
    }

    private void TestAllDivs(BigInteger i1, BigInteger i2)
    {
        TestDiv(i1, i2);
        TestDiv(-i1, i2);
        TestDiv(i1, -i2);
        TestDiv(-i1, -i2);
    }
}