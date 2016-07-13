using System;
using System.Collections.Generic;
using System.Text;


namespace hjn20160520.Common
{
    /// <summary>
    /// ���Сдת���Ĵ�д��(����һ������ת��д)��
    /// ����֧�ֵ����ڣ�С������֧�ֵ���(������λ������Banker���뷨����)
    /// </summary>
    public class NumGetString
    {
        private static String[] Ls_ShZ ={ "��", "Ҽ", "��", "��", "��", "��", "½", "��", "��", "��", "ʰ" };
        private static String[] Ls_DW_Zh ={ "Ԫ", "ʰ", "��", "Ǫ", "��", "ʰ", "��", "Ǫ", "��", "ʰ", "��", "Ǫ", "��" };
        private static String[] Num_DW ={ "","ʰ", "��", "Ǫ", "��", "ʰ", "��", "Ǫ", "��", "ʰ", "��", "Ǫ", "��" };
        private static String[] Ls_DW_X ={ "��", "��" };

        /// <summary>
        /// ���Сдת���Ĵ�д��
        /// ����֧�ֵ����ڣ�С������֧�ֵ���(������λ������Banker���뷨����)
        /// </summary>
        /// <param name="Num">��Ҫת����˫���ȸ�����</param>
        /// <returns>ת������ַ���</returns>
        public static String NumGetStr(decimal Num)
        {
            Boolean iXSh_bool = false;//�Ƿ���С����Ĭ��û��(0����Ϊû��)
            Boolean iZhSh_bool = true;//�Ƿ�������,Ĭ����(0����Ϊû��)

            string NumStr;//���������ַ���
            string NumStr_Zh;//��������
            string NumSr_X = "";//С������
            string NumStr_DQ;//��ǰ�������ַ�
            string NumStr_R = "";//���ص��ַ���

            Num = Math.Round(Num, 2);//��������ȡ��λ

            //���ַ������������
            if (Num < 0)
                return "��ת��Ƿ��";
            if (Num > 9999999999999.99m)
                return "��������˭������ô��Ǯ��";
            if (Num == 0)
                return Ls_ShZ[0];

            //�ж��Ƿ�������
            if (Num < 1.00m)
                iZhSh_bool = false;

            NumStr = Num.ToString();

            NumStr_Zh = NumStr;//Ĭ��ֻ����������
            if (NumStr_Zh.Contains("."))
            {//�ֿ�������С������
                NumStr_Zh = NumStr.Substring(0, NumStr.IndexOf("."));
                NumSr_X = NumStr.Substring((NumStr.IndexOf(".") + 1), (NumStr.Length - NumStr.IndexOf(".") - 1));
                iXSh_bool = true;
            }


            if (NumSr_X == "" || int.Parse(NumSr_X) <= 0)
            {//�ж��Ƿ���С������
                iXSh_bool = false;
            }

            if (iZhSh_bool)
            {//�������ִ���
                NumStr_Zh = Reversion_Str(NumStr_Zh);//��ת�ַ���

                for (int a = 0; a < NumStr_Zh.Length; a++)
                {//��������ת��
                    NumStr_DQ = NumStr_Zh.Substring(a, 1);
                    if (int.Parse(NumStr_DQ) != 0)
                        NumStr_R = Ls_ShZ[int.Parse(NumStr_DQ)] + Ls_DW_Zh[a] + NumStr_R;
                    else if (a == 0 || a == 4 || a == 8)
                    {
                        if (NumStr_Zh.Length > 8 && a == 4)
                            continue;
                        NumStr_R = Ls_DW_Zh[a] + NumStr_R;
                    }
                    else if (int.Parse(NumStr_Zh.Substring(a - 1, 1)) != 0)
                        NumStr_R = Ls_ShZ[int.Parse(NumStr_DQ)] + NumStr_R;

                }

                if (!iXSh_bool)
                    return NumStr_R + "��";

                //NumStr_R += "��";
            }

            for (int b = 0; b < NumSr_X.Length; b++)
            {//С������ת��
                NumStr_DQ = NumSr_X.Substring(b, 1);
                if (int.Parse(NumStr_DQ) != 0)
                    NumStr_R += Ls_ShZ[int.Parse(NumStr_DQ)] + Ls_DW_X[b];
                else if (b != 1 && iZhSh_bool)
                    NumStr_R += Ls_ShZ[int.Parse(NumStr_DQ)];
            }

            return NumStr_R;

        }

        /// <summary>
        /// �ṩһ������ֱ��ת��д����������λ��
        /// </summary>
        /// <param name="NumStr">��Ҫת���������ַ���</param>
        /// <param name="Dw">�Ƿ����λ</param>
        /// <returns>ת������ַ���</returns>
        public static String LowercaseGetCap(String NumStr,Boolean Dw)
        {
            String CapStr="";
            String NumStr_LS;

            if (NumStr == String.Empty)
                return String.Empty;

            if (Dw)
                NumStr = Reversion_Str(NumStr);

            try
            {
                for (Int32 c = 0; c < NumStr.Length; c++)
                {
                    NumStr_LS = NumStr.Substring(c, 1);
                    if (Dw)
                    {
                        if (int.Parse(NumStr_LS) != 0)
                            CapStr = Ls_ShZ[int.Parse(NumStr_LS)] + Num_DW[c] + CapStr;
                        else if (c == 0 || c == 4 || c == 8)
                        {
                            if (NumStr_LS.Length > 8 && c == 4)
                                continue;
                            CapStr = Num_DW[c] + CapStr;
                        }
                        else if (int.Parse(NumStr.Substring(c - 1, 1)) != 0)
                            CapStr = Ls_ShZ[int.Parse(NumStr_LS)] + CapStr;
                    }
                    else
                        CapStr += Ls_ShZ[int.Parse(NumStr_LS)];
                }

                return CapStr;
            }
            catch (Exception Err)
            {
                return "ת������"+Err.Message;
            }
        }

        /// <summary>
        /// ��ת�ַ���
        /// </summary>
        /// <param name="Rstr">��Ҫ��ת���ַ���</param>
        /// <returns>��ת����ַ���</returns>
        private static String Reversion_Str(String Rstr)
        {
            Char[] LS_Str = Rstr.ToCharArray();
            Array.Reverse(LS_Str);
            String ReturnSte = "";
            ReturnSte = new String(LS_Str);//��ת�ַ���

            return ReturnSte;
        }
    }
}
