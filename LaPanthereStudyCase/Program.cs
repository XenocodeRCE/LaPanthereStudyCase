using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaPanthereStudyCase
{
    class Program
    {
        static void Main(string[] args)
        {
            ModuleDefMD module = ModuleDefMD.Load(@"C:\Users\XenocodeRCE\Desktop\TurboPatch_Release.exe");
            Console.WriteLine("[!] " + module.Location);

            Console.WriteLine("[+] Parsing module...");
            //parse ctor module
            ModuleCtorParser(module);

            Console.WriteLine("[+] Writting file ...");
            module.Write(@"C:\Users\XenocodeRCE\Desktop\_________________.exe");
            Console.ReadKey();
        }



        public static void ModuleCtorParser(ModuleDefMD module)
        {
            var mb = module.GlobalType.FindStaticConstructor();
            var instr = mb.Body.Instructions;

            for (int i = 0; i < instr.Count - 1; i++)
            {
                if(instr[i].OpCode == OpCodes.Ldstr)
                {
                    if(instr[i+1].Operand is FieldDef)
                    {
                        var field = (FieldDef)instr[i + 1].Operand;
                        ReplaceCtorLDSTR(module, instr[i].Operand as string, field);
                    }
                }
                else if(instr[i].OpCode == OpCodes.Ldc_I4)
                {
                    if (instr[i + 1].Operand is FieldDef)
                    {
                        var field = (FieldDef)instr[i + 1].Operand;
                        ReplaceCtorLDCI4(module, instr[i].GetLdcI4Value(), field);
                    }
                }
            }
        }


        public static void ReplaceCtorLDSTR(ModuleDefMD module, string value, FieldDef field)
        {
            foreach(TypeDef type in module.GetTypes())
            {
                if(!type.IsGlobalModuleType)
                {
                    foreach(MethodDef method in type.Methods)
                    {
                        if(method.HasBody && method.Body.HasInstructions)
                        {
                            foreach(var instr in method.Body.Instructions)
                            {
                                if(instr.OpCode == OpCodes.Ldsfld)
                                {
                                    if(instr.Operand is FieldDef && instr.Operand == field)
                                    {
                                        instr.OpCode = OpCodes.Ldstr;
                                        instr.Operand = value;
                                    } 
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void ReplaceCtorLDCI4(ModuleDefMD module, int value, FieldDef field)
        {
            foreach (TypeDef type in module.GetTypes())
            {
                if (!type.IsGlobalModuleType)
                {
                    foreach (MethodDef method in type.Methods)
                    {
                        if (method.HasBody && method.Body.HasInstructions)
                        {
                            foreach (var instr in method.Body.Instructions)
                            {
                                if (instr.OpCode == OpCodes.Ldsfld)
                                {
                                    if (instr.Operand is FieldDef && instr.Operand == field)
                                    {
                                        instr.OpCode = OpCodes.Ldc_I4;
                                        instr.Operand = value;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
