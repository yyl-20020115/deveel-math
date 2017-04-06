﻿using System;

namespace Deveel.Math {
	static class BigDecimalMath {
		public static BigDecimal DivideBigIntegers(BigInteger scaledDividend, BigInteger scaledDivisor, int scale,
			RoundingMode roundingMode) {
			BigInteger remainder;
			BigInteger quotient = BigMath.DivideAndRemainder(scaledDividend, scaledDivisor, out remainder);
			if (remainder.Sign == 0) {
				return new BigDecimal(quotient, scale);
			}
			int sign = scaledDividend.Sign * scaledDivisor.Sign;
			int compRem; // 'compare to remainder'
			if (scaledDivisor.BitLength < 63) {
				// 63 in order to avoid out of long after <<1
				long rem = remainder.ToInt64();
				long divisor = scaledDivisor.ToInt64();
				compRem = BigDecimal.LongCompareTo(System.Math.Abs(rem) << 1, System.Math.Abs(divisor));
				// To look if there is a carry
				compRem = BigDecimal.RoundingBehavior(BigInteger.TestBit(quotient, 0) ? 1 : 0,
					sign * (5 + compRem), roundingMode);

			} else {
				// Checking if:  remainder * 2 >= scaledDivisor 
				compRem = BigMath.Abs(remainder).ShiftLeftOneBit().CompareTo(BigMath.Abs(scaledDivisor));
				compRem = BigDecimal.RoundingBehavior(BigInteger.TestBit(quotient, 0) ? 1 : 0,
					sign * (5 + compRem), roundingMode);
			}
			if (compRem != 0) {
				if (quotient.BitLength < 63) {
					return BigDecimal.ValueOf(quotient.ToInt64() + compRem, scale);
				}
				quotient += BigInteger.FromInt64(compRem);
				return new BigDecimal(quotient, scale);
			}
			// Constructing the result with the appropriate unscaled value
			return new BigDecimal(quotient, scale);
		}

		public static BigDecimal DividePrimitiveLongs(long scaledDividend, long scaledDivisor, int scale,
			RoundingMode roundingMode) {
			long quotient = scaledDividend / scaledDivisor;
			long remainder = scaledDividend % scaledDivisor;
			int sign = System.Math.Sign(scaledDividend) * System.Math.Sign(scaledDivisor);
			if (remainder != 0) {
				// Checking if:  remainder * 2 >= scaledDivisor
				int compRem; // 'compare to remainder'
				compRem = BigDecimal.LongCompareTo(System.Math.Abs(remainder) << 1, System.Math.Abs(scaledDivisor));
				// To look if there is a carry
				quotient += BigDecimal.RoundingBehavior(((int)quotient) & 1, sign * (5 + compRem), roundingMode);
			}
			// Constructing the result with the appropriate unscaled value
			return BigDecimal.ValueOf(quotient, scale);
		}

		public static BigDecimal Divide(BigDecimal dividend, BigDecimal divisor) {
			BigInteger p = dividend.GetUnscaledValue();
			BigInteger q = divisor.GetUnscaledValue();
			BigInteger gcd; // greatest common divisor between 'p' and 'q'
			BigInteger quotient;
			BigInteger remainder;
			long diffScale = (long)dividend._scale - divisor._scale;
			int newScale; // the new scale for final quotient
			int k; // number of factors "2" in 'q'
			int l = 0; // number of factors "5" in 'q'
			int i = 1;
			int lastPow = BigDecimal.FivePow.Length - 1;

			if (divisor.IsZero) {
				// math.04=Division by zero
				throw new ArithmeticException(Messages.math04); //$NON-NLS-1$
			}
			if (p.Sign == 0) {
				return BigDecimal.GetZeroScaledBy(diffScale);
			}
			// To divide both by the GCD
			gcd = BigMath.Gcd(p, q);
			p = p / gcd;
			q = q / gcd;
			// To simplify all "2" factors of q, dividing by 2^k
			k = q.LowestSetBit;
			q = q >> k;
			// To simplify all "5" factors of q, dividing by 5^l
			do {
				quotient = BigMath.DivideAndRemainder(q, BigDecimal.FivePow[i], out remainder);
				if (remainder.Sign == 0) {
					l += i;
					if (i < lastPow) {
						i++;
					}
					q = quotient;
				} else {
					if (i == 1) {
						break;
					}
					i = 1;
				}
			} while (true);
			// If  abs(q) != 1  then the quotient is periodic
			if (!BigMath.Abs(q).Equals(BigInteger.One)) {
				// math.05=Non-terminating decimal expansion; no exact representable decimal result.
				throw new ArithmeticException(Messages.math05); //$NON-NLS-1$
			}
			// The sign of the is fixed and the quotient will be saved in 'p'
			if (q.Sign < 0) {
				p = -p;
			}
			// Checking if the new scale is out of range
			newScale = BigDecimal.ToIntScale(diffScale + System.Math.Max(k, l));
			// k >= 0  and  l >= 0  implies that  k - l  is in the 32-bit range
			i = k - l;

			p = (i > 0)
				? Multiplication.MultiplyByFivePow(p, i)
				: p << -i;
			return new BigDecimal(p, newScale);
		}

		public static BigDecimal Divide(BigDecimal dividend, BigDecimal divisor, MathContext mc) {
			/* Calculating how many zeros must be append to 'dividend'
			 * to obtain a  quotient with at least 'mc.precision()' digits */
			long traillingZeros = mc.Precision + 2L
			                      + divisor.AproxPrecision() - dividend.AproxPrecision();
			long diffScale = (long)dividend._scale - divisor._scale;
			long newScale = diffScale; // scale of the final quotient
			int compRem; // to compare the remainder
			int i = 1; // index   
			int lastPow = BigDecimal.TenPow.Length - 1; // last power of ten
			BigInteger integerQuot; // for temporal results
			BigInteger quotient = dividend.GetUnscaledValue();
			BigInteger remainder;
			// In special cases it reduces the problem to call the dual method
			if ((mc.Precision == 0) || (dividend.IsZero) || (divisor.IsZero))
				return Divide(dividend, divisor);

			if (traillingZeros > 0) {
				// To append trailing zeros at end of dividend
				quotient = dividend.GetUnscaledValue() * Multiplication.PowerOf10(traillingZeros);
				newScale += traillingZeros;
			}
			quotient = BigMath.DivideAndRemainder(quotient, divisor.GetUnscaledValue(), out remainder);
			integerQuot = quotient;
			// Calculating the exact quotient with at least 'mc.precision()' digits
			if (remainder.Sign != 0) {
				// Checking if:   2 * remainder >= divisor ?
				compRem = remainder.ShiftLeftOneBit().CompareTo(divisor.GetUnscaledValue());
				// quot := quot * 10 + r;     with 'r' in {-6,-5,-4, 0,+4,+5,+6}
				integerQuot = (integerQuot * BigInteger.Ten) +
				              BigInteger.FromInt64(quotient.Sign * (5 + compRem));
				newScale++;
			} else {
				// To strip trailing zeros until the preferred scale is reached
				while (!BigInteger.TestBit(integerQuot, 0)) {
					quotient = BigMath.DivideAndRemainder(integerQuot, BigDecimal.TenPow[i], out remainder);
					if ((remainder.Sign == 0)
					    && (newScale - i >= diffScale)) {
						newScale -= i;
						if (i < lastPow) {
							i++;
						}
						integerQuot = quotient;
					} else {
						if (i == 1) {
							break;
						}
						i = 1;
					}
				}
			}
			// To perform rounding
			return new BigDecimal(integerQuot, BigDecimal.ToIntScale(newScale), mc);
		}

		public static BigDecimal DivideToIntegralValue(BigDecimal dividend, BigDecimal divisor) {
			BigInteger integralValue; // the integer of result
			BigInteger powerOfTen; // some power of ten
			BigInteger quotient;
			BigInteger remainder;
			long newScale = (long)dividend._scale - divisor._scale;
			long tempScale = 0;
			int i = 1;
			int lastPow = BigDecimal.TenPow.Length - 1;

			if (divisor.IsZero) {
				// math.04=Division by zero
				throw new ArithmeticException(Messages.math04); //$NON-NLS-1$
			}
			if ((divisor.AproxPrecision() + newScale > dividend.AproxPrecision() + 1L)
			    || (dividend.IsZero)) {
				/* If the divisor's integer part is greater than this's integer part,
				 * the result must be zero with the appropriate scale */
				integralValue = BigInteger.Zero;
			} else if (newScale == 0) {
				integralValue = dividend.GetUnscaledValue() / divisor.GetUnscaledValue();
			} else if (newScale > 0) {
				powerOfTen = Multiplication.PowerOf10(newScale);
				integralValue = dividend.GetUnscaledValue() / (divisor.GetUnscaledValue() * powerOfTen);
				integralValue = integralValue * powerOfTen;
			} else {
				// (newScale < 0)
				powerOfTen = Multiplication.PowerOf10(-newScale);
				integralValue = (dividend.GetUnscaledValue() * powerOfTen) / divisor.GetUnscaledValue();
				// To strip trailing zeros approximating to the preferred scale
				while (!BigInteger.TestBit(integralValue, 0)) {
					quotient = BigMath.DivideAndRemainder(integralValue, BigDecimal.TenPow[i], out remainder);
					if ((remainder.Sign == 0)
					    && (tempScale - i >= newScale)) {
						tempScale -= i;
						if (i < lastPow) {
							i++;
						}
						integralValue = quotient;
					} else {
						if (i == 1) {
							break;
						}
						i = 1;
					}
				}
				newScale = tempScale;
			}
			return ((integralValue.Sign == 0)
				? BigDecimal.GetZeroScaledBy(newScale)
				: new BigDecimal(integralValue, BigDecimal.ToIntScale(newScale)));
		}

		public static BigDecimal DivideToIntegralValue(BigDecimal dividend, BigDecimal divisor, MathContext mc) {
			int mcPrecision = mc.Precision;
			int diffPrecision = dividend.Precision - divisor.Precision;
			int lastPow = BigDecimal.TenPow.Length - 1;
			long diffScale = (long)dividend._scale - divisor._scale;
			long newScale = diffScale;
			long quotPrecision = diffPrecision - diffScale + 1;
			BigInteger quotient;
			BigInteger remainder;
			// In special cases it call the dual method
			if ((mcPrecision == 0) || (dividend.IsZero) || (divisor.IsZero)) {
				return DivideToIntegralValue(dividend, divisor);
			}
			// Let be:   this = [u1,s1]   and   divisor = [u2,s2]
			if (quotPrecision <= 0) {
				quotient = BigInteger.Zero;
			} else if (diffScale == 0) {
				// CASE s1 == s2:  to calculate   u1 / u2 
				quotient = dividend.GetUnscaledValue() / divisor.GetUnscaledValue();
			} else if (diffScale > 0) {
				// CASE s1 >= s2:  to calculate   u1 / (u2 * 10^(s1-s2)  
				quotient = dividend.GetUnscaledValue() / (divisor.GetUnscaledValue() * Multiplication.PowerOf10(diffScale));
				// To chose  10^newScale  to get a quotient with at least 'mc.precision()' digits
				newScale = System.Math.Min(diffScale, System.Math.Max(mcPrecision - quotPrecision + 1, 0));
				// To calculate: (u1 / (u2 * 10^(s1-s2)) * 10^newScale
				quotient = quotient * Multiplication.PowerOf10(newScale);
			} else {
				// CASE s2 > s1:   
				/* To calculate the minimum power of ten, such that the quotient 
				 *   (u1 * 10^exp) / u2   has at least 'mc.precision()' digits. */
				long exp = System.Math.Min(-diffScale, System.Math.Max((long)mcPrecision - diffPrecision, 0));
				long compRemDiv;
				// Let be:   (u1 * 10^exp) / u2 = [q,r]  
				quotient = BigMath.DivideAndRemainder(dividend.GetUnscaledValue() * Multiplication.PowerOf10(exp),
					divisor.GetUnscaledValue(), out remainder);
				newScale += exp; // To fix the scale
				exp = -newScale; // The remaining power of ten
				// If after division there is a remainder...
				if ((remainder.Sign != 0) && (exp > 0)) {
					// Log10(r) + ((s2 - s1) - exp) > mc.precision ?
					compRemDiv = (new BigDecimal(remainder)).Precision
					             + exp - divisor.Precision;
					if (compRemDiv == 0) {
						// To calculate:  (r * 10^exp2) / u2
						remainder = (remainder * Multiplication.PowerOf10(exp)) / divisor.GetUnscaledValue();
						compRemDiv = System.Math.Abs(remainder.Sign);
					}
					if (compRemDiv > 0) {
						// The quotient won't fit in 'mc.precision()' digits
						// math.06=Division impossible
						throw new ArithmeticException(Messages.math06); //$NON-NLS-1$
					}
				}
			}
			// Fast return if the quotient is zero
			if (quotient.Sign == 0) {
				return BigDecimal.GetZeroScaledBy(diffScale);
			}
			BigInteger strippedBI = quotient;
			BigDecimal integralValue = new BigDecimal(quotient);
			long resultPrecision = integralValue.Precision;
			int i = 1;
			// To strip trailing zeros until the specified precision is reached
			while (!BigInteger.TestBit(strippedBI, 0)) {
				quotient = BigMath.DivideAndRemainder(strippedBI, BigDecimal.TenPow[i], out remainder);
				if ((remainder.Sign == 0) &&
				    ((resultPrecision - i >= mcPrecision)
				     || (newScale - i >= diffScale))) {
					resultPrecision -= i;
					newScale -= i;
					if (i < lastPow) {
						i++;
					}
					strippedBI = quotient;
				} else {
					if (i == 1) {
						break;
					}
					i = 1;
				}
			}
			// To check if the result fit in 'mc.precision()' digits
			if (resultPrecision > mcPrecision) {
				// math.06=Division impossible
				throw new ArithmeticException(Messages.math06); //$NON-NLS-1$
			}
			integralValue._scale = BigDecimal.ToIntScale(newScale);
			integralValue.SetUnscaledValue(strippedBI);
			return integralValue;
		}


		public static BigDecimal Divide(BigDecimal dividend, BigDecimal divisor, int scale, RoundingMode roundingMode) {
			// Let be: this = [u1,s1]  and  divisor = [u2,s2]
			if (divisor.IsZero) {
				// math.04=Division by zero
				throw new ArithmeticException(Messages.math04); //$NON-NLS-1$
			}

			long diffScale = ((long)dividend._scale - divisor._scale) - scale;
			if (dividend._bitLength < 64 && divisor._bitLength < 64) {
				if (diffScale == 0)
					return DividePrimitiveLongs(dividend.smallValue, divisor.smallValue, scale, roundingMode);
				if (diffScale > 0) {
					if (diffScale < BigDecimal.LongTenPow.Length &&
					    divisor._bitLength + BigDecimal.LongTenPowBitLength[(int)diffScale] < 64) {
						return DividePrimitiveLongs(dividend.smallValue, divisor.smallValue * BigDecimal.LongTenPow[(int)diffScale], scale, roundingMode);
					}
				} else {
					// diffScale < 0
					if (-diffScale < BigDecimal.LongTenPow.Length &&
					    dividend._bitLength + BigDecimal.LongTenPowBitLength[(int)-diffScale] < 64) {
						return DividePrimitiveLongs(dividend.smallValue * BigDecimal.LongTenPow[(int)-diffScale], divisor.smallValue, scale, roundingMode);
					}

				}
			}
			BigInteger scaledDividend = dividend.GetUnscaledValue();
			BigInteger scaledDivisor = divisor.GetUnscaledValue(); // for scaling of 'u2'

			if (diffScale > 0) {
				// Multiply 'u2'  by:  10^((s1 - s2) - scale)
				scaledDivisor = Multiplication.MultiplyByTenPow(scaledDivisor, (int)diffScale);
			} else if (diffScale < 0) {
				// Multiply 'u1'  by:  10^(scale - (s1 - s2))
				scaledDividend = Multiplication.MultiplyByTenPow(scaledDividend, (int)-diffScale);
			}
			return DivideBigIntegers(scaledDividend, scaledDivisor, scale, roundingMode);
		}

		public static BigDecimal DivideAndRemainder(BigDecimal dividend, BigDecimal divisor, out BigDecimal remainder) {
			var quotient = DivideToIntegralValue(dividend, divisor);
			remainder = dividend.Subtract(quotient.Multiply(divisor));
			return quotient;
		}

		public static BigDecimal DivideAndRemainder(BigDecimal dividend, BigDecimal divisor, MathContext mc, out BigDecimal remainder) {
			var quotient = BigDecimalMath.DivideToIntegralValue(dividend, divisor, mc);
			remainder = dividend.Subtract(quotient.Multiply(divisor));
			return quotient;
		}


		public static BigDecimal Pow(BigDecimal number, int n) {
			if (n == 0) {
				return BigDecimal.One;
			}
			if ((n < 0) || (n > 999999999)) {
				// math.07=Invalid Operation
				throw new ArithmeticException(Messages.math07); //$NON-NLS-1$
			}
			long newScale = number._scale * (long)n;
			// Let be: this = [u,s]   so:  this^n = [u^n, s*n]
			return ((number.IsZero)
				? BigDecimal.GetZeroScaledBy(newScale)
				: new BigDecimal(BigMath.Pow(number.GetUnscaledValue(), n), BigDecimal.ToIntScale(newScale)));
		}

		public static BigDecimal Pow(BigDecimal number, int n, MathContext mc) {
			// The ANSI standard X3.274-1996 algorithm
			int m = System.Math.Abs(n);
			int mcPrecision = mc.Precision;
			int elength = (int)System.Math.Log10(m) + 1; // decimal digits in 'n'
			int oneBitMask; // mask of bits
			BigDecimal accum; // the single accumulator
			MathContext newPrecision = mc; // MathContext by default

			// In particular cases, it reduces the problem to call the other 'pow()'
			if ((n == 0) || ((number.IsZero) && (n > 0))) {
				return Pow(number, n);
			}
			if ((m > 999999999) || ((mcPrecision == 0) && (n < 0))
			    || ((mcPrecision > 0) && (elength > mcPrecision))) {
				// math.07=Invalid Operation
				throw new ArithmeticException(Messages.math07); //$NON-NLS-1$
			}
			if (mcPrecision > 0) {
				newPrecision = new MathContext(mcPrecision + elength + 1,
					mc.RoundingMode);
			}
			// The result is calculated as if 'n' were positive        
			accum = BigMath.Round(number, newPrecision);
			oneBitMask = Utils.HighestOneBit(m) >> 1;

			while (oneBitMask > 0) {
				accum = accum.Multiply(accum, newPrecision);
				if ((m & oneBitMask) == oneBitMask) {
					accum = accum.Multiply(number, newPrecision);
				}
				oneBitMask >>= 1;
			}
			// If 'n' is negative, the value is divided into 'ONE'
			if (n < 0) {
				accum = Divide(BigDecimal.One, accum, newPrecision);
			}
			// The final value is rounded to the destination precision
			accum.InplaceRound(mc);
			return accum;
		}

		public static BigDecimal Scale(BigDecimal number, int newScale, RoundingMode roundingMode) {
			long diffScale = newScale - (long)number._scale;
			// Let be:  'number' = [u,s]        
			if (diffScale == 0) {
				return number;
			}
			if (diffScale > 0) {
				// return  [u * 10^(s2 - s), newScale]
				if (diffScale < BigDecimal.LongTenPow.Length &&
				    (number._bitLength + BigDecimal.LongTenPowBitLength[(int)diffScale]) < 64) {
					return BigDecimal.ValueOf(number.smallValue * BigDecimal.LongTenPow[(int)diffScale], newScale);
				}
				return new BigDecimal(Multiplication.MultiplyByTenPow(number.GetUnscaledValue(), (int)diffScale), newScale);
			}
			// diffScale < 0
			// return  [u,s] / [1,newScale]  with the appropriate scale and rounding
			if (number._bitLength < 64 && -diffScale < BigDecimal.LongTenPow.Length) {
				return BigDecimalMath.DividePrimitiveLongs(number.smallValue, BigDecimal.LongTenPow[(int)-diffScale], newScale, roundingMode);
			}

			return DivideBigIntegers(number.GetUnscaledValue(), Multiplication.PowerOf10(-diffScale), newScale, roundingMode);
		}

		public static BigDecimal MovePoint(BigDecimal number, long newScale) {
			if (number.IsZero) {
				return BigDecimal.GetZeroScaledBy(System.Math.Max(newScale, 0));
			}
			/* When:  'n'== Integer.MIN_VALUE  isn't possible to call to movePointRight(-n)  
			 * since  -Integer.MIN_VALUE == Integer.MIN_VALUE */
			if (newScale >= 0) {
				if (number._bitLength < 64) {
					return BigDecimal.ValueOf(number.smallValue, BigDecimal.ToIntScale(newScale));
				}
				return new BigDecimal(number.GetUnscaledValue(), BigDecimal.ToIntScale(newScale));
			}
			if (-newScale < BigDecimal.LongTenPow.Length &&
			    number._bitLength + BigDecimal.LongTenPowBitLength[(int)-newScale] < 64) {
				return BigDecimal.ValueOf(number.smallValue * BigDecimal.LongTenPow[(int)-newScale], 0);
			}
			return new BigDecimal(Multiplication.MultiplyByTenPow(number.GetUnscaledValue(), (int)-newScale), 0);
		}
	}
}
