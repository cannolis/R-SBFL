/************************************************************************
 * 
 * class: Metrics
 * 
 * 功能：实现基于语句的各个算法公式，
 *       沿用Naish的 A Model for Spectra-based Software Diagnosis
 *       中的符号定义，称算法的经验公式为ranking metrics
 *       
 * 符号：
 *      P:    成功用例数
 *      a_ep: 覆盖语句a的成功用例数
 *      a_np: 没有覆盖语句a的成功用例数，a_np = P - a_ep
 *      F:    失败用例数
 *      a_ef: 覆盖语句a的失败用例数
 *      a_nf: 没有覆盖语句a的失败用例数，a_nf = F - a_ef
 * 
 * *************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaultLocalization
{
    public class FLMetrics    
    {   
        public delegate double MethodDelegate(double a_ef, double a_ep, double a_nf, double a_np);

        private static Dictionary<string, MethodDelegate> methodDictionary = null;

        private static void InitialDictionary()
        {
            methodDictionary = new Dictionary<string, MethodDelegate>();

            // ER1:Op1, Op2

            methodDictionary.Add("Op1", Op1);

            methodDictionary.Add("Op2", Op2);

            // ER2: Jaccard, Anderberg, Sorensen-Dice, Dice and Goodman

            methodDictionary.Add("Jaccard", Jaccard);

            methodDictionary.Add("Anderberg", Anderberg);

            methodDictionary.Add("Sqrensen_Dice", Sqrensen_Dice);

            methodDictionary.Add("Dice", Dice);

            methodDictionary.Add("Goodman", Goodman);

            // ER3: Tarantula qe CBI Inc
            methodDictionary.Add("Tarantula", t_color);

            methodDictionary.Add("qe", qe);

            methodDictionary.Add("CBI_Inc", CBI_Inc);


            // ER4: Wong2, Hamman, Simple Matching, Sokal, Rogers & Tanimoto, Hanmming Eclid
            
            methodDictionary.Add("Wong2", Wong2);
            
            methodDictionary.Add("Hamann", Hamann);
            
            methodDictionary.Add("Simple_Matching", Simple_Matching);
            
            methodDictionary.Add("Sokal", Sokal);
            
            methodDictionary.Add("Rogers_and_Tanimoto", Rogers_and_Tanimoto);
            
            methodDictionary.Add("Hamming", Hamming);
            
            methodDictionary.Add("Euclid", Euclid);

            // ER5: Wong1, Russell & Rao, Binary
            
            methodDictionary.Add("Wong1", Wong1);
            
            methodDictionary.Add("Russel_And_Rao", Russel_And_Rao);
            
            methodDictionary.Add("Binary", Binary);

            methodDictionary.Add("Binary2", Binary2);

            // ER6: Scott, Rogot1

            methodDictionary.Add("Scott", Scott);
            
            methodDictionary.Add("Rogot1", Rogot1);

            // others
            methodDictionary.Add("Kulczynski1", Kulczynski1);

            methodDictionary.Add("Kulczynski2", Kulczynski2);

            methodDictionary.Add("M1", M1_Everitt);

            methodDictionary.Add("M2", M2_Everitt);

            methodDictionary.Add("Ochiai", Ochiai);
                        
            methodDictionary.Add("Ample2", Ample2);
            
            methodDictionary.Add("Wong3", Wong3);
            
            methodDictionary.Add("Arithmetic_Mean", Arithmetic_Mean);

            methodDictionary.Add("Cohen", Cohen);
            
            methodDictionary.Add("Fleiss", Fleiss);
            
            methodDictionary.Add("Zoltar", Zoltar);
            
            methodDictionary.Add("Ochiai2", Ochiai2);
            
            methodDictionary.Add("Harmonic_Mean", Harmonic_Mean);
            
            methodDictionary.Add("CrossTab", CrossTab);

            // Heuristic
            methodDictionary.Add("Heuristic_a", Heuristic_a);
            methodDictionary.Add("Heuristic_b", Heuristic_b);
            methodDictionary.Add("Heuristic_c", Heuristic_c);
            // DStar
            methodDictionary.Add("Dstar1", Dstar1);
            methodDictionary.Add("Dstar2", Dstar2);
            methodDictionary.Add("Dstar3", Dstar3);
            methodDictionary.Add("Dstar4", Dstar4);
            methodDictionary.Add("Dstar5", Dstar5);
            methodDictionary.Add("Dstar6", Dstar6);
            methodDictionary.Add("Dstar7", Dstar7);
            methodDictionary.Add("Dstar8", Dstar8);
            methodDictionary.Add("Dstar9", Dstar9);
            methodDictionary.Add("Dstar10", Dstar10);

            // Weight 李成龙添加
            methodDictionary.Add("Weight4", Weight4);

            // Constant 李成龙添加
            methodDictionary.Add("Constant", Constant);
        }

        /// <summary>
        /// 获取算法入口
        /// </summary>
        /// <param name="sMethodName">算法名称</param>
        /// <returns>算法入口</returns>
        public static MethodDelegate GetFormula(string sMethodName)
        {
            if (null == methodDictionary)
            {
                InitialDictionary();
            }

            return new MethodDelegate(methodDictionary[sMethodName]);
        }

        #region Formulas

        #region ER1: Op1, Op2
        //ER1(Op1,Op2)-Op1
        public static double Op1(double a_ef, double a_ep, double a_nf, double a_np)
        {

            if (a_nf > 0)
            {
                return -1;
            }
            else
            {
                return a_np;
            }
        }

        //ER1(Op1,Op2)-Op2
        public static double Op2(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            result = a_ef - a_ep / (a_ep + a_np + 1);
            return result;
        }

        #endregion

        #region ER2: Jaccard, Anderberg, Sorensen-Dice, Dice and Goodman

        //ER2(Jaccard, Anderberg, Sqrensen-Dice, Dice, Goodman)-Jaccard
        //Jaccard算法
        public static double Jaccard(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if ( (a_ef + a_nf + a_ep) != 0)
            {
                result = a_ef / (a_ef + a_nf + a_ep);
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //ER2(Jaccard, Anderberg, Sqrensen-Dice, Dice, Goodman)-Anderberg
        //Anderberg算法
        public static double Anderberg(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if ( (a_ef + 2 * (a_nf + a_ep)) == 0 )
            {
                result = 0;
            }
            else
            {
                result = a_ef / (a_ef + 2 * (a_nf + a_ep));
            }
            return result;
        }

        //ER2(Jaccard, Anderberg, Sqrensen-Dice, Dice, Goodman)-Sqrensen-Dice
        //Sqrensen_Dice算法
        public static double Sqrensen_Dice(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if ( (2*a_ef + a_nf + a_ep) != 0)
            {
                result = 2 * a_ef / (2 * a_ef + a_nf + a_ep);
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //ER2(Jaccard, Anderberg, Sqrensen-Dice, Dice, Goodman)-Dice
        //Dice算法
        public static double Dice(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if ( (a_ef + a_nf + a_ep) != 0)
            {
                result = 2 * a_ef / (a_ef + a_nf + a_ep);
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //ER2(Jaccard, Anderberg, Sqrensen-Dice, Dice, Goodman)-Goodman
        //Goodman算法
        public static double Goodman(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if ( (2*a_ef + a_nf + a_ep) != 0)
            {
                result = (2*a_ef - a_nf - a_ep) / (2*a_ef + a_nf + a_ep);
            }
            else
            {
                result = 0;
            }
            return result;
        }

        #endregion

        #region ER3: Tarantula qe CBI Inc

        //ER3(Tarantula, qe, CBI Inc)-Tarantula
        //Tarantula算法
        //可疑度一计算方法
        public static double t_color(double a_ef, double a_ep, double a_nf, double a_np)
        {
            double P = a_ep + a_np;
            double F = a_ef + a_nf;

            double result = 0;

            if ( P == 0 || F == 0)
            {
                result = 0;
            }
            else
            {
                double passed_percent = a_ep / P;
                double failed_percent = a_ef / F;
                if ((passed_percent + failed_percent) != 0)
                {
                    result = failed_percent / (passed_percent + failed_percent);
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }
        //可疑度二计算方法
        public static double t_bright(double a_ef, double a_ep, double a_nf, double a_np)
        {
            double P = a_ep + a_np;
            double F = a_ef + a_nf;

            double result = 0;

            if (P == 0 || F == 0)
            {
                result = 0;
            }
            else
            {
                double passed_percent = a_ep / P;
                double failed_percent = a_ef / F;
                result = Math.Max(passed_percent, failed_percent);
            }
            return result;
        }

        //ER3(Tarantula, qe, CBI Inc)-qe
        //qe算法
        public static double qe(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if ((a_ef + a_ep) == 0)
            {
                result = 0;
            }
            else
            {
                result = a_ef / (a_ef + a_ep);
            }
            return result;
        }

        //ER3(Tarantula, qe, CBI Inc)-CBI Inc
        //CBI Inc算法
        public static double CBI_Inc(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if (a_ef + a_ep == 0)
            {
                result = 0;
            }
            else
            {
                result = a_ef / (a_ef + a_ep) - (a_ef + a_nf) / ( a_ef + a_nf + a_ep + a_np );
            }
            return result;
        }

        #endregion

        #region ER4: Wong2, Hamman, Simple Matching, Sokal, Rogers & Tanimoto, Hanmming Eclid

        //ER4(Wong2, Hamann, Simple Matching, Sokal, Rogers & Tanimoto, Hamming etc., Euclid)-Wong2
        //Wong2算法
        public static double Wong2(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            result = a_ef - a_ep;
            return result;
        }

        //ER4(Wong2, Hamann, Simple Matching, Sokal, Rogers & Tanimoto, Hamming etc., Euclid)-Hamann
        //Hamann算法
        public static double Hamann(double a_ef, double a_ep, double a_nf, double a_np)
        {
            double P = a_ep + a_np;
            double F = a_ef + a_nf;

            double result = 0;

            if ( P + F != 0)
            {
                result = (a_ef + a_np - a_nf - a_ep) / (P + F);
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //ER4(Wong2, Hamann, Simple Matching, Sokal, Rogers & Tanimoto, Hamming etc., Euclid)-Simple Matching
        //Simple_Matching算法
        public static double Simple_Matching(double a_ef, double a_ep, double a_nf, double a_np)
        {
            double P = a_ep + a_np;
            double F = a_ef + a_nf;

            double result = 0;

            if ( P + F != 0)
            {
                result = (a_ef + a_np) / (P + F);
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //ER4(Wong2, Hamann, Simple Matching, Sokal, Rogers & Tanimoto, Hamming etc., Euclid)-Sokal
        //Sokal算法
        public static double Sokal(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if ( (2*(a_ef + a_np) + a_nf + a_ep) != 0)
            {
                result = 2 * (a_ef + a_np) / (2 * (a_ef + a_np) + a_nf + a_ep);
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //ER4(Wong2, Hamann, Simple Matching, Sokal, Rogers & Tanimoto, Hamming etc., Euclid)-Rogers & Tanimoto
        //Rogers_and_Tanimoto算法
        public static double Rogers_and_Tanimoto(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if ((a_ef + a_np + 2*(a_nf + a_ep)) != 0)
            {
                result = (a_ef + a_np) / (a_ef + a_np + 2*(a_nf + a_ep));
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //ER4(Wong2, Hamann, Simple Matching, Sokal, Rogers & Tanimoto, Hamming etc., Euclid)-Hamming etc.
        //Hamming算法
        public static double Hamming(double a_ef, double a_ep, double a_nf, double a_np)
        {


            return a_ef + a_np;
        }

        //ER4(Wong2, Hamann, Simple Matching, Sokal, Rogers & Tanimoto, Hamming etc., Euclid)-Euclid
        //Euclid算法
        public static double Euclid(double a_ef, double a_ep, double a_nf, double a_np)
        {


            return Math.Sqrt(a_ef + a_np);
        }

        #endregion

        #region ER5: Wong1, Russell & Rao, Binary

        //ER5(Wong1,Russel & Rao,Binary)-Wong1
        //Wong1算法
        public static double Wong1(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            result = a_ef;
            return result;
        }

        //ER5(Wong1,Russel & Rao,Binary)-Russel & Rao
        //Russel & Rao算法
        public static double Russel_And_Rao(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double P = a_ep + a_np;
            double F = a_ef + a_nf;
            double result = 0;
            if ((P + F) != 0)
                result = a_ef / (P + F);
            else
                result = 0;
            return result;
        }

        //ER5(Wong1,Russel & Rao,Binary)-Binary
        //Binary算法
        public static double Binary(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if (a_nf > 0)
            {
                result = 0;
            }
            else
            {
                result = 1;
            }
            return result;
        }


        public static double Binary2(double a_ef, double a_ep, double a_nf, double a_np)
        {
            double P = a_ep + a_np;
            double F = a_ef + a_nf;
            double result = 0;

            if (a_ef < F)
                result = 0;
            else if (a_ef == F)
                result = 1;

            return result;
        }

        #endregion

        #region ER6: Scott, Rogot1

        //ER6(Scott, Rogot1)-Scott
        //Scott算法
        public static double Scott(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if ( ((2*a_ef + a_nf + a_ep) * (2*a_np + a_nf + a_ep)) != 0)
            {
                result = (4 * a_ef * a_np - 4 * a_nf * a_ep - Math.Pow((a_nf - a_ep), 2)) / ((2 * a_ef + a_nf + a_ep) * (2 * a_np + a_nf + a_ep));
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //ER6(Scott, Rogot1)-Rogot1
        //Rogot1算法
        public static double Rogot1(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if ((2 * a_ef + a_nf + a_ep) * (2 * a_np + a_nf + a_ep) != 0)
            {
                result = 0.5 * (a_ef / (2 * a_ef + a_nf + a_ep) + a_np / (2 * a_np + a_nf + a_ep));
            }
            else
            {
                result = 0;
            }
            return result;
        }

        #endregion


        #region  李成龙添加 用于验证权重效果
        //Weight4
        public static double Weight4(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if (a_ep != 0)
            {
                result = a_ef / a_ep;
            }
            else
            {
                result = 2 * a_ef;
            }
            return result;
        }

        //Constant
        public static double Constant(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 1;

            return result;
        }

        #endregion 

        //Kulczynski1算法
        public static double Kulczynski1(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if ((a_nf + a_ep) != 0)
            {
                result = a_ef / (a_nf + a_ep);
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //Kulczynski2算法
        public static double Kulczynski2(double a_ef, double a_ep, double a_nf, double a_np)
        {
            double P = a_ep + a_np;
            double F = a_ef + a_nf;
            double result = 0;

            if (( F != 0) && ((a_ef + a_ep) != 0))
            {
                result = 0.5 * ((a_ef / (a_nf + a_ef)) + (a_ef / (a_ef + a_ep)));
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //M1_Everitt算法
        public static double M1_Everitt(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if ((a_nf + a_ep) != 0)
            {
                result = (a_ef + a_np) / (a_nf + a_ep);
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //M2_Everitt算法
        public static double M2_Everitt(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if ( (a_ef + a_np + 2*(a_nf + a_ep)) != 0)
            {
                result = a_ef / (a_ef + a_np + 2 * (a_nf + a_ep));
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //Ochiai算法
        public static double Ochiai(double a_ef, double a_ep, double a_nf, double a_np)
        {
            double P = a_ep + a_np;
            double F = a_ef + a_nf;
            double result = 0;

            if (F != 0 && (a_ef + a_ep) != 0 && a_ef != 0)
            {
                result = a_ef / Math.Sqrt((a_ef + a_nf) * (a_ef + a_ep));
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //Overlap算法
        public static double Overlap(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if (Math.Min(Math.Min(a_ef, a_nf), a_ep) != 0)
            {
                result = a_ef / (Math.Min(Math.Min(a_ef, a_nf), a_ep));
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //Zoltar算法
        public static double Zoltar(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if (a_ef != 0)
            {
                if ((a_ef + a_nf + a_ep + 10000 * a_nf * a_ep / a_ef) != 0)
                {
                    result = a_ef / (a_ef + a_nf + a_ep + 10000 * a_nf * a_ep / a_ef);
                }
                else
                {
                    result = 0;
                }
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //Ample算法
        public static double Ample(double a_ef, double a_ep, double a_nf, double a_np)
        {
            double P = a_ep + a_np;
            double F = a_ef + a_nf;
            double result = 0;

            if (F != 0 && P != 0)
            {
                result = Math.Abs(a_ef / F - a_ep / P);
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //Ample2算法
        public static double Ample2(double a_ef, double a_ep, double a_nf, double a_np)
        {
            double P = a_ep + a_np;
            double F = a_ef + a_nf;

            double result = 0;

            if (F != 0 && P != 0)      
            {
                result = (a_ef / F) - (a_ep / P);
            }
            else if (F == 0 && P != 0)      // 李成龙改
            {
                result = - (a_ep / P);
            }
            else if (F != 0 && P == 0)
            {
                result = a_ef / F;
            }
            else
            {
                result = 0;
            }

            return result;
        }

        //Wong3算法
        public static double Wong3(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if (a_ep <= 2)
            {
                result = a_ef - a_ep;
            }
            else if (a_ep <= 10)
            {
                result = a_ef - 2 - 0.1 * (a_ep - 2);
            }
            else
            {
                result = a_ef - 2.8 - 0.001 * (a_ep - 10);
            }
            return result;
        }

        //Ochiai2算法
        public static double Ochiai2(double a_ef, double a_ep, double a_nf, double a_np)
        {
            double P = a_ep + a_np;
            double F = a_ef + a_nf;

            double result = 0;

            if ((a_ef + a_ep) * (a_nf + a_np) * F * P != 0)
            {
                result = (a_ef * a_np) / (Math.Sqrt((a_ef + a_ep) * (a_nf + a_np) * F * P));
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //Geometric_Mean算法
        public static double Geometric_Mean(double a_ef, double a_ep, double a_nf, double a_np)
        {
            double P = a_ep + a_np;
            double F = a_ef + a_nf;
            double result = 0;

            if (((a_ef + a_ep) * (a_nf + a_np) * F * P) != 0)
            {
                result = (a_ef * a_np - a_nf * a_ep) / (Math.Sqrt((a_ef + a_ep) * (a_nf + a_np) * F * P));
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //Harmonic_Mean算法
        public static double Harmonic_Mean(double a_ef, double a_ep, double a_nf, double a_np)
        {
            double P = a_ep + a_np;
            double F = a_ef + a_nf;

            double result = 0;

            if (((a_ef + a_ep) * (a_nf + a_np) * F * P) != 0)
            {
                result = (a_ef * a_np - a_nf * a_ep) * ((a_ef + a_ep) * (a_np + a_nf) + F * P) / ((a_ef + a_ep) * (a_nf + a_np) * F * P);
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //Arithmetic_Mean算法
        public static double Arithmetic_Mean(double a_ef, double a_ep, double a_nf, double a_np)
        {
            double P = a_ep + a_np;
            double F = a_ef + a_nf;

            double result = 0;

            if (((a_ef + a_ep) * (a_nf + a_np) * F * P) != 0)
            {
                result = (2 * a_ef * a_np - 2 * a_nf * a_ep) / ((a_ef + a_ep) * (a_nf + a_np) * F * P);
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //Cohen算法
        public static double Cohen(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if (((a_ef + a_ep) * (a_np + a_ep) + (a_ef + a_nf) * (a_nf + a_np))!= 0)
            {
                result = (2 * a_ef * a_np - 2 * a_nf * a_ep) / ((a_ef + a_ep) * (a_np + a_ep) + (a_ef + a_nf) * (a_nf + a_np));
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //Fleiss算法
        public static double Fleiss(double a_ef, double a_ep, double a_nf, double a_np)
        {

            double result = 0;

            if (((2 * a_ef + a_nf + a_ep) + (2 * a_np + a_nf + a_ep)) != 0)
            {
                result = (4 * a_ef * a_np - 4 * a_nf * a_ep - Math.Pow((a_nf - a_ep),2)) / ((2 * a_ef + a_nf + a_ep) + (2 * a_np + a_nf + a_ep));
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //Rogot2算法
        public double Rogot2(double a_ef, double a_ep, double a_nf, double a_np)
        {
            double P = a_ep + a_np;
            double F = a_ef + a_nf;

            double result = 0;

            if (F != 0 && P != 0 && (a_ef + a_ep) != 0 && (a_np+ a_nf) != 0)
            {
                result = 0.25 * (a_ef / (a_ef + a_ep) + a_ef / F + a_np / P + a_np / (a_np + a_nf));
            }
            else
            {
                result = 0;
            }
            return result;
        }

        //crosstab算法
        public static double CrossTab(double a_ef, double a_ep, double a_nf, double a_np)
        {
            double P = a_ep + a_np;
            double F = a_ef + a_nf;
            double result = 0;
            double X_w1, X_w2, X_w3, X_w4;

            double E_cf = (a_ep + a_ef) * F / (P + F);
            double E_cs = (a_ep + a_ef) * P / (P + F);
            double E_uf = (a_np + a_nf) * F / (P + F);
            double E_us = (a_np + a_nf) * P / (P + F);


            if (E_cf != 0) { X_w1 = Math.Pow(a_ef - E_cf, 2) / E_cf; } else { X_w1 = 0; };   //李成龙改
            if (E_cs != 0) { X_w2 = Math.Pow(a_ep - E_cs, 2) / E_cs; } else { X_w2 = 0; };
            if (E_uf != 0) { X_w3 = Math.Pow(a_nf - E_uf, 2) / E_uf; } else { X_w3 = 0; };
            if (E_us != 0) { X_w4 = Math.Pow(a_np - E_us, 2) / E_us; } else { X_w4 = 0; };
            double X_w = X_w1 + X_w2 + X_w3 + X_w4;

            if (a_ep == 0)
            {
                if (a_ef == 0)
                {
                    result = 0;

                }
                else
                {
                    result = X_w;
                }
            }
            else
            {
                double Y_w = (a_ef / F) / (a_ep / P);

                if (Y_w > 1)
                {
                    result = X_w;
                }
                else if (Y_w < 1)
                {
                    result = -X_w;
                }
                else
                {
                    result = 0;
                }

            }

            return result;
        }

        #region Heuristic算法
        private static double Heuristic_abc(double a_ef, double a_ep, double a_nf, double a_np, double alpha)
        {
            double P = a_ep + a_np;
            double F = a_ef + a_nf;

            double result = 0;

            double n_F1 = 0;
            if (0 == a_ef)
                n_F1 = 0;
            else if (1 == a_ef)
                n_F1 = 1;
            else if (a_ef >= 2)
                n_F1 = 2;

            double n_F2 = 0;
            if (a_ef <= 2)
                n_F2 = 0;
            else if (3 <= a_ef && a_ef <= 6)
                n_F2 = a_ef - 2;
            else if (a_ef > 6)
                n_F2 = 4;

            double n_F3 = 0;
            if (a_ef <= 6)
                n_F3 = 0;
            else if (a_ef > 6)
                n_F3 = a_ef - 6;

            double n_S1 = 0;
            if (n_F1 == 0 || n_F1 == 1)
                n_S1 = 0;
            else if (n_F1 == 2 && a_ep >= 1)
                n_S1 = 1;

            double n_S2 = 0;
            if (a_ep <= n_S1)
                n_S2 = 0;
            else if (n_S1 < a_ep && a_ep < (n_S1 + n_F2))   // 李成龙改 对照原文发现的小问题
                n_S2 = a_ep - n_S1;
            else if (a_ep >= (n_S1 + n_F2))
                n_S2 = n_F2;

            double n_S3 = 0;
            if (a_ep < (n_S1 + n_S2))
                n_S3 = 0;
            else if (a_ep >= (n_S1 + n_S2))
                n_S3 = a_ep - n_S1 - n_S2;

            result = (1.0 * n_F1 + 0.1 * n_F2 + 0.01 * n_F3) - (1.0 * n_S1 + 0.1 * n_S2 + alpha * F / P * n_S3);

            return result;
        }

        public static double Heuristic_a(double a_ef, double a_ep, double a_nf, double a_np)
        {
            return Heuristic_abc(a_ef, a_ep, a_nf, a_np, 0.01);
        }

        public static double Heuristic_b(double a_ef, double a_ep, double a_nf, double a_np)
        {
            return Heuristic_abc(a_ef, a_ep, a_nf, a_np, 0.001);
        }

        public static double Heuristic_c(double a_ef, double a_ep, double a_nf, double a_np)
        {
            return Heuristic_abc(a_ef, a_ep, a_nf, a_np, 0.0001);
        }
        #endregion

        #region D*算法
        private static double Dstar(double a_ef, double a_ep, double a_nf, double a_np, int star)
        {

            double result = 0;

            double a_ef_star = 1.0;
            for (int i = 0; i < star; i++)
                a_ef_star *= a_ef;

            double tmp = a_nf + a_ep;
            if (0 == tmp)
                result = 2 * a_ef_star;             // 李成龙改
            else
                result = a_ef_star / tmp;

            return result;
        }

        public static double Dstar1(double a_ef, double a_ep, double a_nf, double a_np)
        {
            return Dstar(a_ef, a_ep, a_nf, a_np, 1);
        }

        public static double Dstar2(double a_ef, double a_ep, double a_nf, double a_np)
        {
            return Dstar(a_ef, a_ep, a_nf, a_np, 2);
        }

        public static double Dstar3(double a_ef, double a_ep, double a_nf, double a_np)
        {
            return Dstar(a_ef, a_ep, a_nf, a_np, 3);
        }

        public static double Dstar4(double a_ef, double a_ep, double a_nf, double a_np)
        {
            return Dstar(a_ef, a_ep, a_nf, a_np, 4);
        }

        public static double Dstar5(double a_ef, double a_ep, double a_nf, double a_np)
        {
            return Dstar(a_ef, a_ep, a_nf, a_np, 5);
        }

        public static double Dstar6(double a_ef, double a_ep, double a_nf, double a_np)
        {
            return Dstar(a_ef, a_ep, a_nf, a_np, 6);
        }

        public static double Dstar7(double a_ef, double a_ep, double a_nf, double a_np)
        {
            return Dstar(a_ef, a_ep, a_nf, a_np, 7);
        }

        public static double Dstar8(double a_ef, double a_ep, double a_nf, double a_np)
        {
            return Dstar(a_ef, a_ep, a_nf, a_np, 8);
        }

        public static double Dstar9(double a_ef, double a_ep, double a_nf, double a_np)
        {
            return Dstar(a_ef, a_ep, a_nf, a_np, 9);
        }

        public static double Dstar10(double a_ef, double a_ep, double a_nf, double a_np)
        {
            return Dstar(a_ef, a_ep, a_nf, a_np, 10);
        }
        #endregion


        #endregion
    }

}
