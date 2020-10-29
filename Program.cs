using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Calculator
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = ReadFile();
            input = CheckImput(input);

            if (input != "False")
            {
                File.WriteAllText("../../../output.txt", FinalCalculations(input));
                Console.WriteLine("Пример успешно вычислен!");
            }
        }

        static char[] listSign = { '+', '-', '*', '/', '^', '^', '^' };
        static Dictionary<string, int> orderOfSigns = new Dictionary<string, int>
        {
            {"+", 1},
            {"-", 1},
            {"*", 2},
            {"/", 2},
            {"^", 3}
        };

        static string ReadFile()
        {
            try
            {
                if (File.ReadLines("../../../input.txt").Count() == 1) return File.ReadAllText("../../../input.txt");
                else
                {
                    Console.WriteLine("Неверное количество строк в файле (должно быть 1)");
                    return "False";
                }
            } catch
            {
                Console.WriteLine("Файл не обнаружен (поместите файл input.txt в папку с программой)");
                return "False";
            }
        }

        static string CheckImput(string str)
        {
            if (str == "False") return "False";

            int parenthesisNum = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '(') parenthesisNum++;
                if (str[i] == ')') parenthesisNum--;

                if (!Char.IsDigit(str[i]) && !listSign.Contains(str[i]) && 
                    str[i] != '(' && str[i] != ')' && str[i] != ' ' && 
                    str[i] != '\r' && str[i] != '\n')
                {
                    Console.WriteLine("Неизвестный символ " + str[i] + " под номером " + (i + 1));
                    return "False";
                }

                if (i != str.Length - 1)
                    if (listSign.Contains(str[i]) && listSign.Contains(str[i + 1]) && str[i + 1] != '-' )
                    {
                        Console.WriteLine("Ошибка в постановке знаков (знак " + str[i] + " под номером " + (i + 1) + " не содержит второго операнда)");
                        return "False";
                    }
            }

            if (parenthesisNum != 0)
            {
                Console.WriteLine("Количество открывающихся и закрывающихся скобок не совпадает");
                return "False";
            }

            return str;
        }

        //вычисляет выражения в скобках, заменяя скобку результатом, пока не останется простое выражение без скобок
        static string FinalCalculations(string str)
        {
            List<string> operations = ParseEquation(str);

            for (int i = operations.Count - 1; i >= 0; i--)
            {
                for (int ii = 0; ii < operations[i].Length; ii++)
                {
                    if (operations[i][ii] == '(')
                    {
                        
                        int parenthesisNum = 0;
                        for (int iii = ii; iii < operations[i].Length;)
                        {
                            if (operations[i][iii] == '(') parenthesisNum++;
                            if (operations[i][iii] == ')') parenthesisNum--;
                            if (parenthesisNum > 0) operations[i] = operations[i].Remove(iii, 1);
                            else
                            {
                                operations[i] = operations[i].Remove(iii, 1);
                                break;
                            }
                        }

                        operations[i] = operations[i].Insert(ii, operations[i + 1]);
                        operations.RemoveAt(i + 1);
                    }
                }

                operations[i] = Convert.ToString(Calculation(operations[i]));
            }

            return operations[0];
        }

        //Делит выражение на действия, опираясь на скобки
        static List<string> ParseEquation(string str)
        {
            List<string> operations = new List<string>() { str };

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '(')
                {
                    string local = "";
                    int parenthesisNum = 1;
                    for (int ii = i + 1; ii < str.Length; ii++)
                    {
                        if (str[ii] == '(') parenthesisNum++;
                        if (str[ii] == ')') parenthesisNum--;
                        if (parenthesisNum != 0) local += str[ii];
                        else break;
                    }

                    operations.Add(local);
                }
            }

            return operations;
        }

        //Вычисление выражения без скобок
        static decimal Calculation(string str)
        {
            List<decimal> digits = GetDigits(str);
            List<string> signs = GetSigns(str);
            List<int> order = GetOrder(signs);
            int pos;

            if (order.Count > 0)
            {
                do
                {
                    pos = -1;
                    for (int i = 0; i < order.Count; i++)
                        if (order[i] == 3)
                        {
                            pos = i;
                            break;
                        }

                    if (pos != -1)
                    {
                        switch (signs[pos])
                        {
                            case "+":
                                digits[pos] += digits[pos + 1];
                                break;
                            case "-":
                                digits[pos] -= digits[pos + 1];
                                break;
                            case "*":
                                digits[pos] *= digits[pos + 1];
                                break;
                            case "/":
                                digits[pos] /= digits[pos + 1];
                                break;
                            case "^":
                                digits[pos] = (decimal)Math.Pow((double)digits[pos], (double)digits[pos + 1]);
                                break;
                        }


                        digits.RemoveAt(pos + 1);
                        signs.RemoveAt(pos);
                        order.RemoveAt(pos);
                    }
                    else
                        for (int i = 0; i < order.Count; i++) order[i]++;
                } while (digits.Count != 1);
            }

            return digits[0];
        }

        //Создаёт лист цифр выражения без скобок
        static List<decimal> GetDigits(string str)
        {
            List<decimal> digits = new List<decimal>();

            for (int i = 0; i < str.Length; i++)
            {
                if (Char.IsDigit(str[i]))
                {
                    string local = "";

                    if (i != 0)
                        if (str[i - 1] == '-')
                            if (i - 1 != 0)
                            {
                                if (!Char.IsDigit(str[i - 2])) local += '-';
                            }
                            else
                                local += '-';

                    for (; i < str.Length; i++)
                    {
                        if (Char.IsDigit(str[i]) || str[i] == ',') local += str[i];
                        else break;
                    }

                    digits.Add(Convert.ToDecimal(local));
                }
            }

            return digits;
        }
      
        //Создаёт лист из знаков выражения без скобок
        static List<string> GetSigns(string str)
        {
            List<string> signs = new List<string>();

            str = DeliteMinus(str);

            for (int i = 0; i < str.Length; i++)
            {
                if (listSign.Contains(str[i]))
                {
                    signs.Add(Convert.ToString(str[i]));
                }
            }

            return signs;
        }

        //Удаляет из выражения минусы, которые не являются операторами
        static string DeliteMinus(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '-')
                    if (i == 0)
                    {
                        str = str.Remove(i, 1);
                        i--;
                    }
                    else
                    if (!Char.IsDigit(str[i - 1]))
                    {
                        str = str.Remove(i, 1);
                        i--;
                    }
            }

            return str;
        }

        //Задаёт порядок вычисления выражения без скобок
        static List<int> GetOrder(List<string> signs)
        {
            List<int> order = new List<int>();

            for (int i = 0; i < signs.Count; i++)
            {
                order.Add(orderOfSigns[signs[i]]);
            }

            return order;
        }

    }
}
