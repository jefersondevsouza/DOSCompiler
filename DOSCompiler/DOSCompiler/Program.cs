using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DOSCompiler
{
    class Program
    {
        #region Variáveis

        static string InicioFrase = "***** ";
        static object locker = new object();
        static Dictionary<string, Parametros> parametros = new Dictionary<string, Parametros>();
        static string trunkMainDisco = @"Trunk\Main";
        static string trunkMainTeam = @"Trunk/Main";
        static string branchDisco = @"Branch\{0}";
        static string branchTeam = @"Branch/{0}";
        static bool trunk = false;
        static string versaoDisco = string.Empty;
        static string versaoTeam = string.Empty;
        static string versao = string.Empty;

        /*
         2019 = C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe
         2019 = C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\TF.exe
         2017 = C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe
         2017 = C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\TF.exe
         */

        static string MSBuildLocation = string.Empty;
        static string MSTFLocation = string.Empty;
        static string LocalVersoes = string.Empty;

        private static int ct = 0;
        public static int ContadorThread
        {
            set
            {
                lock (locker)
                {
                    ct = value;
                }
            }
            get
            {
                return ct;
            }
        }

        #endregion

        static void Main(string[] args)
        {
            LerArquivo();

            Console.WriteLine(InicioFrase);
            Console.WriteLine("**************************************Project Compiler by Jefers********************************************");
            Console.WriteLine(InicioFrase);
            Console.WriteLine(InicioFrase);
            Console.WriteLine(InicioFrase + "(Ctrl + c) para finalizar");
            Console.WriteLine(InicioFrase);
            Console.WriteLine(InicioFrase);
            Console.Write(InicioFrase + "Deseja compilar o trunk? S ou N: ");
            string resposta = Console.ReadLine();

            if (resposta.ToUpper().Equals("S"))
            {
                trunk = true;
            }
            else
            {
                EscolherVersao();
            }

            if (trunk)
            {
                versaoDisco = trunkMainDisco;
                versaoTeam = trunkMainTeam;
            }
            else
            {
                versaoDisco = string.Format(branchDisco, versao);
                versaoTeam = string.Format(branchTeam, versao);
            }

            AtualizarCaminhos();
            PrepararCompilar();

            Console.WriteLine(InicioFrase);
            Console.WriteLine(InicioFrase);
            Console.WriteLine("**************************************Project Compiler by Jefers********************************************");
            Console.WriteLine(InicioFrase + "Tecle para finalizar");
            Console.Read();
        }

        #region Preparar

        private static void LerArquivo()
        {
            string nomeArquivo = "Projetos.txt";

            string file = File.ReadAllText(nomeArquivo);

            using (StreamReader s = new StreamReader(nomeArquivo))
            {
                int cod = 1;
                while (!s.EndOfStream)
                {
                    string linha = s.ReadLine();

                    if (linha.ToUpper().StartsWith("MSBUILDPATH"))
                    {
                        MSBuildLocation = linha.Substring(12);
                    }
                    else if (linha.ToUpper().StartsWith("MSTFPATH"))
                    {
                        MSTFLocation = linha.Substring(9);
                    }
                    else if (linha.ToUpper().StartsWith("VERSIONPATH"))
                    {
                        LocalVersoes = linha.Substring(12);
                    }

                    else
                    {
                        Parametros p = new Parametros();

                        string[] array = linha.Split(',');

                        if (array.Length > 0)
                        {
                            p.Codigo = cod.ToString();
                            p.LocalDisco = array[0];
                            p.LocalTeamFoundation = array[1];

                        }

                        if (!parametros.ContainsKey(p.Codigo))
                        {
                            parametros.Add(p.Codigo.Trim(), p);
                        }

                        cod++;
                    }

                }
            }
        }

        private static void AtualizarCaminhos()
        {
            foreach (var p in parametros.Values )
            {
                p.LocalDisco = string.Format(p.LocalDisco, versaoDisco);
                p.LocalTeamFoundation = string.Format(p.LocalTeamFoundation, versaoTeam);
            }
        }

        private static void EscolherVersao()
        {
            Dictionary<int, string> versoesDisponiveis = new Dictionary<int, string>();
            string pastaWorkspace = LocalVersoes; //@"C:\workspace\ErpTpjUaisoft\Branch";
            if (Directory.Exists(pastaWorkspace))
            {
                Console.WriteLine(InicioFrase + "Versões encontradas: ");

                DirectoryInfo di = new DirectoryInfo(pastaWorkspace);
                var diretorios = di.GetDirectories();


                int cod = 1;
                foreach (var diretorio in diretorios)
                {
                    Console.WriteLine(InicioFrase + string.Format("{0} - {1}", cod, diretorio.Name));
                    versoesDisponiveis.Add(cod, diretorio.Name);
                    cod++;
                }

                if (versoesDisponiveis.Count == 0)
                {
                    Console.WriteLine(InicioFrase + "Versões encontradas = 0 ");
                }
                else
                {
                    Console.WriteLine(InicioFrase + "* Pode informar o código da versão ou digitar a versão completa. NÃO precisa ser apenar as listadas. ");
                }
            }

            Console.Write(InicioFrase + "Informe a versão: ");
            versao = Console.ReadLine();

            int codVersao = 0;
            int.TryParse(versao, out codVersao);
            if (versoesDisponiveis.ContainsKey(codVersao))
            {
                versao = versoesDisponiveis[codVersao];
            }

            Console.Write(InicioFrase + "Confirma a versão: " + versao + "? S ou N: ");
            var resposta = Console.ReadLine();

            if (resposta.ToUpper() != "S")
                EscolherVersao();
        }

        private static List<Parametros> EscolherProjeto()
        {
            List<Parametros> listaParam = new List<Parametros>();
            Console.WriteLine(Environment.NewLine + InicioFrase + "* + parte do nome");
            Console.WriteLine(InicioFrase + ", separar os código");
            Console.WriteLine(InicioFrase + "; compilar tudo");
            Console.WriteLine(InicioFrase + "- compilar no intervalo de códigos");
            Console.WriteLine(InicioFrase);
            Console.Write(InicioFrase + "Digite o(s) código(s) da solution: ");
            List<string> solutions = new List<string>();
            string solution = Console.ReadLine();

            if (solution.ToUpper().Contains("-"))
            {
                int codInic = int.Parse(solution.Substring(0, solution.IndexOf("-")));
                int codFim = int.Parse(solution.Substring(solution.IndexOf("-") + 1));

                List<int> codigos = new List<int>();
                for (int i = codInic; i <= codFim; i++)
                {
                    codigos.Add(i);
                }

                solution = string.Join(",", codigos);
            }


            if (solution.ToUpper().StartsWith("*"))
            {
                solution = solution.Replace("*", "");
                if (solution.Contains(","))
                {
                    var arraySolution = solution.Split(new char[] { ',' });
                    foreach (string parteSolution in arraySolution)
                    {
                        foreach (var p in parametros)
                        {
                            if (p.Value.Projeto.ToUpper().EndsWith(parteSolution.ToUpper()))
                            {
                                listaParam.Add(p.Value);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var p in parametros)
                    {
                        if (p.Value.Projeto.ToUpper().EndsWith(solution.ToUpper()))
                        {
                            listaParam.Add(p.Value);
                            break;
                        }
                    }
                }

                if (listaParam.Count == 0)
                {
                    Console.Write(InicioFrase + "Código inválido!");
                    return EscolherProjeto();
                }
            }

            else if (solution.ToUpper().Equals(";"))
            {
                foreach (var p in parametros)
                {
                    listaParam.Add(p.Value);
                }
            }

            else
            {
                if (solution.Contains(","))
                {
                    var arraySolution = solution.Split(new char[] { ',' });
                    for (int i = 0; i < arraySolution.Length; i++)
                    {

                        solution = arraySolution[i];
                        if (solution.Trim().Length > 0)
                        {
                            if (parametros.ContainsKey(solution))
                            {
                                var parametro = parametros[solution];
                                listaParam.Add(parametros[solution]);
                            }
                            else
                            {
                                Console.Write(InicioFrase + "Algum Código inválido!");
                                return EscolherProjeto();
                            }
                        }

                        if (listaParam.Count == 0)
                        {
                            Console.Write(InicioFrase + "Nenhum Código informado!");
                            return EscolherProjeto();
                        }
                    }
                }
                else
                {
                    if (parametros.ContainsKey(solution.Trim()))
                    {
                        listaParam.Add(parametros[solution]);
                    }
                    else
                    {
                        Console.Write(InicioFrase + "Código inválido!");
                        return EscolherProjeto();
                    }
                }
            }

            if (listaParam.Count > 0)
            {
                Console.WriteLine(InicioFrase + "Projeto(s) a compilar: ");
                foreach (var parametro in listaParam)
                {
                    Console.WriteLine(InicioFrase + parametro.Projeto);
                }

                Console.Write(InicioFrase + "Confirme a compilação do(s) projeto(s): " + "? S ou N: ");

                var resposta = Console.ReadLine();

                if (resposta.ToUpper() != "S")
                    EscolherProjeto();
            }

            return listaParam;
        }

        #endregion

        #region Compilar

        private static void PrepararCompilar()
        {
            try
            {
                ListaEConfigurarCompilacao();
            }
            catch (ArgumentException ex)
            {
                Console.Write(InicioFrase);
                Console.Write(InicioFrase + ex.Message);
                Console.Write(InicioFrase);
            }

            Console.Write(InicioFrase + "Deseja compilar outro projeto? S ou N: ");
            var resposta = Console.ReadLine();

            if (resposta.ToUpper() == "S")
                PrepararCompilar();

        }

        private static void ListaEConfigurarCompilacao()
        {
            Console.WriteLine(InicioFrase);
            Console.Write(InicioFrase + "Informe uma das solutions: " + Environment.NewLine);
            Console.WriteLine(InicioFrase);

            foreach (var parametro in parametros)
            {
                Console.WriteLine(InicioFrase + InicioFrase + string.Format("{0} [{1}]", parametro.Value.Codigo, parametro.Value.Projeto));
            }

            List<Parametros> solutionsCompilar = EscolherProjeto();

            bool get = ConferirGetLast();
            if (get)
            {
                ProcessarGet(solutionsCompilar);
            }


            PercorrerECompilar(solutionsCompilar);

        }

        private static void PercorrerECompilar(List<Parametros> solutionsCompilar)
        {
            List<Parametros> solutionsVoltarNoFinal = new List<Parametros>();

            for (int i = 0; i < solutionsCompilar.Count; i++)
            {
                Parametros s = solutionsCompilar[i];

                Compilar(s);

                s.ZerarParametros();
                AnalisarCompilacao(s);

                if (s.CompilarOutraSolutionAgora)
                {
                    s.ZerarParametros();
                    ListaEConfigurarCompilacao();
                    i--;
                }
                else if (s.VoltarNelaNoFinal)
                {
                    s.ZerarParametros();
                    solutionsVoltarNoFinal.Add(s);
                }
                else if (s.RecompilarAgora)
                {
                    s.ZerarParametros();
                    i--;
                }

                s.ZerarParametros();
            }

            if (solutionsVoltarNoFinal.Count > 0)
                PercorrerECompilar(solutionsVoltarNoFinal);
        }

        private static void AnalisarCompilacao(Parametros s)
        {
            if (!s.Builded)
            {
                Console.WriteLine(InicioFrase + "Você pode *PARAR a compilação");
                Console.WriteLine(InicioFrase + "Você pode *CONTINUAR com a compilação e ao *FINAL essa solução será compilada novamente");
                Console.WriteLine(InicioFrase + "Você pode *ESCOLHER outra solução para compilar e em seguida essa solution será compilada novamente");
                Console.WriteLine(InicioFrase + "Você pode *REPETIR a compilação desta solução e continar, se tiver mais soluções.");
                Console.WriteLine(InicioFrase + "Você pode *ABRIR a solução e fazer alguma correção no código e voltar nessa parte.");
                Console.WriteLine(InicioFrase);
                Console.Write(InicioFrase + "Digite a letra posterior ao * para definir sua escolha: ");
                string continuar = Console.ReadLine();

                if (continuar.ToUpper().Equals("P"))
                {
                    throw new ArgumentException("Compilação abortada por escolha do usuário.");
                }
                else if (continuar.ToUpper().Equals("C"))
                {
                    s.VoltarNelaNoFinal = true;
                }
                else if (continuar.ToUpper().Equals("E"))
                {
                    Console.WriteLine(InicioFrase + "Quer compilar outra solução e voltar nessa? S ou N: ");
                    string compilarDeNovo = Console.ReadLine();
                    if (compilarDeNovo.ToUpper().Equals("S"))
                    {
                        s.CompilarOutraSolutionAgora = true;
                    }
                }
                else if (continuar.ToUpper().Equals("R"))
                {
                    s.RecompilarAgora = true;
                }
                else if (continuar.ToUpper().Equals("A"))
                {
                    Process.Start(s.LocalDisco);
                    Console.WriteLine(InicioFrase + "Abrindo solution...");
                    Console.WriteLine(InicioFrase + s.LocalDisco);
                    Console.WriteLine(InicioFrase + "Voltando para as opções...");
                    AnalisarCompilacao(s);

                }
                else
                {
                    Console.WriteLine(InicioFrase);
                    Console.WriteLine(InicioFrase + "Escolha inválida");
                    Console.WriteLine(InicioFrase);

                    AnalisarCompilacao(s);
                }
            }
        }

        private static void Compilar(Parametros solution)
        {
            if (!solution.Builded)
            {
                solution.NBuild++;

                string conf = string.Format(@"{0}", solution.LocalDisco);
                //var processo = Process.Start(@"C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe ", conf + @" /fl1 /fl2 /fl3 /flp2:logfile=JustErrors.log;errorsonly /flp3:logfile=JustWarnings.log;warningsonly");
                string comando = conf + @" /fl1 /fl2 /fl3 /flp2:logfile=JustErrors.log;errorsonly /flp3:logfile=JustWarnings.log;warningsonly";
                var processo = Process.Start(MSBuildLocation + " ", comando);

                Console.WriteLine(InicioFrase);
                Console.WriteLine(InicioFrase + string.Format("Build nº {0} - Projeto {1}", solution.NBuild, solution.Projeto));
                Console.WriteLine(InicioFrase + "Aguarde...");

                processo.WaitForExit();
                solution.Builded = true;

                string conteudoErro = string.Empty;

                if (File.Exists("JustErrors.log"))
                {
                    conteudoErro = File.ReadAllText("JustErrors.log");
                    if (conteudoErro.Length > 0)
                    {
                        Console.WriteLine(InicioFrase + "ERRO...............................................................................................");

                        Console.WriteLine(conteudoErro);

                        Console.WriteLine(InicioFrase + "FIM ERRO...............................................................................................");

                        solution.Builded = false;
                        solution.Erro.Append(conteudoErro);
                    }
                    else
                    {
                        Console.WriteLine(InicioFrase + "Done...");
                    }
                }
                Console.WriteLine(InicioFrase);
            }
        }

        #endregion

        #region Get Last

        private static void ProcessarGet(List<Parametros> solutionsCompilar)
        {
            int getCount = 1;
            int maxGet = 7;
            foreach (var s in solutionsCompilar)
            {
                ContadorThread++;

                ParameterizedThreadStart p = new ParameterizedThreadStart(GetLast);
                Thread t = new Thread(p);
                t.Start(s);

                if (getCount == maxGet)
                {
                    while (ContadorThread > 0)
                    {
                        Thread.Sleep(300);
                    }

                    getCount = 0;
                }

                getCount++;
            }

            while (ContadorThread > 0)
            {
                Thread.Sleep(300);
            }
        }

        private static bool ConferirGetLast()
        {
            Console.Write(InicioFrase + "Get last antes? S ou N: ");
            string get = Console.ReadLine();

            if (get.ToUpper().Equals("S"))
            {
                return true;
            }

            return false;
        }

        private static void GetLast(object objSolution)
        {
            Parametros solution = (Parametros)objSolution;
            if (!solution.GetLastedDone)
            {
                Console.WriteLine(InicioFrase);
                Console.WriteLine(InicioFrase + string.Format("Get no Projeto {0}", solution.Projeto));
                Console.WriteLine(InicioFrase + "Aguarde...");

                //var processo = Process.Start(@"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\TF.exe", string.Format(@"get {0}", solution.LocalTeamFoundation) + @"  /force /recursive");
                string comando = string.Format(@"get {0}", solution.LocalTeamFoundation) + @"  /force /recursive";
                var processo = Process.Start(MSTFLocation, comando);


                processo.WaitForExit();

                Console.WriteLine(InicioFrase + "Get " + solution.Projeto + " Done");
                solution.GetLastedDone = true;
            }

            ContadorThread--;
        }

        #endregion

    }

    class Parametros
    {
        public Parametros()
        {
            VoltarNelaNoFinal = false;
            CompilarOutraSolutionAgora = false;
            RecompilarAgora = false;
            NBuild = 0;
            Builded = false;
            GetLastedDone = false;
            Codigo = string.Empty;
            LocalDisco = string.Empty;
            LocalTeamFoundation = string.Empty;
            Erro = new StringBuilder();
        }

        public bool Builded { get; set; }
        public int NBuild { get; set; }
        public bool GetLastedDone { get; set; }
        public string Codigo { get; set; }
        public string LocalDisco { get; set; }
        public string LocalTeamFoundation { get; set; }
        public StringBuilder Erro { get; set; }

        public string Projeto
        {
            get
            {
                string s = LocalTeamFoundation.Substring(LocalTeamFoundation.LastIndexOf(@"/"));
                return s.Replace("/", "").Replace("\"", "");

            }
        }

        public bool VoltarNelaNoFinal { get; set; }

        public bool CompilarOutraSolutionAgora { get; set; }

        public bool RecompilarAgora { get; set; }

        public void ZerarParametros()
        {
            this.VoltarNelaNoFinal = false;
            this.CompilarOutraSolutionAgora = false;
            this.RecompilarAgora = false;
        }
    }

}
