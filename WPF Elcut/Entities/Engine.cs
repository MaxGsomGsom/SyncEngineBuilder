using EngineBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EngineBuilder
{
    [Serializable]
    public class Engine
    {
        [NonSerialized]
        public bool unsafeMode = false;

        [NonSerialized]
        public bool onlyRotor = false;


        public Material statorMaterial = new Material("Материал_0", 1, 1);
        public Material magnetMaterial = new Material("Материал_0", 1, 1);
        public Material rotorMaterial = new Material("Материал_0", 1, 1);
        public Material bashmakMaterial = new Material("Материал_0", 1, 1);

        public Engine() { }

        public void Initialize()
        {
            InitEngine();
        }

        public void Solve()
        {
            SolveStepMain();
            SolveStepRotor();
            SolveStepStator();
            SolveRegulators();
        }

        #region Операции с параметрами

        public List<Parameter> engineParameters = new List<Parameter>();

        public void AddEngineParameter(string name, string shotName, double value, string measure, string help)
        {
            int index = SearchEngineParameter(name);
            if (index == -1)
            {
                engineParameters.Add(new Parameter(name, shotName, value, measure, help));
            }
            else
            {
                engineParameters[index].Value = value;
            }
        }

        public void AddEngineParameter(string name, string shotName, double value, double minValue, double maxValue, double step, string measure, string help)
        {
            int index = SearchEngineParameter(name);
            if (index == -1)
            {
                engineParameters.Add(new Parameter(name, shotName, value, minValue, maxValue, step, measure, help));
            }
            else
            {
                engineParameters[index].Value = value;
                engineParameters[index].MinValue = minValue;
                engineParameters[index].MaxValue = maxValue;
                engineParameters[index].Step = step;
            }
        }

        public int SearchEngineParameter(string name)
        {
            for (int i = 0; i < engineParameters.Count; i++)
            {
                if (engineParameters[i].Name == name)
                {
                    return i;
                }
            }
            return -1;
        }

        public int SearchEngineParameterByShortName(string shortName)
        {
            for (int i = 0; i < engineParameters.Count; i++)
            {
                if (engineParameters[i].ShotName == shortName)
                {
                    return i;
                }
            }
            return -1;
        }


        public bool CheckEngineParameters()
        {
            for (int i = 0; i < engineParameters.Count; i++)
            {
                if (engineParameters[i].Value < 0)
                {
                    return false;
                }
            }
            return true;
        }

        public Parameter GetEngineParameter(string name)
        {
            int index = SearchEngineParameter(name);
            if (index != -1)
            {
                return engineParameters[index];
            }
            else
            {
                throw new Exception("Невозможно найти указанный параметр в двигателе: " + name);
            }
        }

        public Parameter GetParam(string shortName)
        {
            int index = SearchEngineParameterByShortName(shortName);
            if (index != -1)
            {
                return engineParameters[index];
            }
            else
            {
                throw new Exception("Невозможно найти указанный параметр в двигателе: " + shortName);
            }
        }

        public void SetEngineParameter(string name, double value)
        {
            int index = SearchEngineParameter(name);
            if (index != -1)
            {
                engineParameters[index].Value = value;
            }
            else
            {
                throw new Exception("Невозможно найти указанный параметр в двигателе: " + name);
            }
        }
        #endregion

        void InitEngine()
        {
            //общее
            AddEngineParameter("НОМИНАЛЬНЫЙ_МОМЕНТ", "Mn", 24D, 10D, 100D, 1D, "Н*м", Hints.Mn);
            AddEngineParameter("НОМИНАЛЬНЫЙ_ФАЗОВЫЙ_ТОК", "Ifn", 29.2D, 10D, 100D, 1D, "А", Hints.Ifn);
            AddEngineParameter("ЧАСТОТА_ПИТАНИЯ", "f", 50.0D, 10D, 1000.0D, 1D, "Гц", Hints.f);
            AddEngineParameter("ЧИСЛО_ПАР_ПОЛЮСОВ", "p", 2.0D, 1.0D, 8.0D, 1.0D, "", Hints.p);
            AddEngineParameter("УГОЛ_НАГРУЗКИ", "phi", 0.2D, 0.0D, Math.PI / 2, 1.0D, "Рад", Hints.phi);
            AddEngineParameter("КПД", "nu", 0.96D, 0.5D, 1.0D, 0.01D, "", Hints.nu);
            AddEngineParameter("ЧИСЛО_ФАЗ_ДВИГАТЕЛЯ", "m", 3.0D, 0.0D, 50.0D, 1.0D, "", Hints.m);
            AddEngineParameter("МАГНИТНАЯ_ИНДУКЦИЯ_МАГНИТА", "Br", 1.17D, 0.5D, 5.0D, 0.01D, "Вб/м2", Hints.Br);
            AddEngineParameter("КОЭРЦИТИВНАЯ_СИЛА_ПО_ИНДУКЦИИ_МАГНИТА", "Hc", 800000D, 10000D, 1000000D, 1000D, "А/м", Hints.Hc);

            //ротор
            AddEngineParameter("КОЭФФИЦИЕНТ_ИНДУКЦИИ_ПРИ_ХОЛОСТОМ_ХОДЕ", "kb", 0.6D, 0.6D, 0.8D, 0.01D, "", Hints.kb);
            AddEngineParameter("КОЭФФИЦИЕНТ_НАПРЯЖЕННОСТИ_МАГНИТНОГО_ПОЛЯ", "kh", 0.735D, 0.6D, 0.8D, 0.01D, "", Hints.kh);
            AddEngineParameter("КОЭФФИЦИЕНТ_РАССЕИВАНИЯ_ХОЛОСТОГО_ХОДА", "sigma0", 1.15D, 1D, 1.2D, 0.01D, "", Hints.sigma0);
            AddEngineParameter("КОЭФФИЦИЕНТ_ПАДЕНИЯ_МАГНИТНОГО_НАПРЯЖЕНИЯ", "alpha1", 1.15D, 1.1D, 1.2D, 0.01D, "", Hints.alpha1);
            AddEngineParameter("КОЭФФИЦИЕНТ_ПРИВЕДЕНИЯ_МДС_РЕАКЦИИ_ЯКОРЯ", "kad", 0.85D, 0.8D, 0.9D, 0.01D, "", Hints.kad);
            AddEngineParameter("КОЭФФИЦИЕНТ_УДАРНОСТИ", "kud", 1.2D, 1.1D, 1.2D, 0.01D, "", Hints.kud);
            AddEngineParameter("КОЭФФИЦИЕНТ_ФОРМЫ_ПОЛЯ", "kf", 1.11D, 1.0D, 1.2D, 0.01D, "", Hints.kf);
            AddEngineParameter("КРАТНОСТЬ_ТОКА_КЗ", "kkz", 6D, 2D, 8D, 0.25D, "", Hints.kkz);
            AddEngineParameter("КОЭФФИЦИЕНТ_ЗАПОЛНЕНИЯ_МАГНИТА", "kzm", 0.558D, 0.3D, 0.9D, 0.001D, "", Hints.kzm);

            AddEngineParameter("СООТНОШЕНИЕ_ДЛИНЫ_К_ДИАМЕТРУ_МАГНИТА", "lambdaM", 1.11D, 0.5D, 1.5D, 0.1D, "", Hints.lambdaM);
            AddEngineParameter("КОЭФФИЦИЕНТ_ЗАПАСА_НА_ИСПОЛЬЗОВАНИЕ_МАГНИТА", "kzap", 1.25D, 1D, 1.5D, 0.05D, "", Hints.kzap);
            AddEngineParameter("КОЭФФИЦИЕНТ_ПОЛЮСНОГО_ПЕРЕКРЫТИЯ", "alphaM", 0.986D, 0.3D, 1D, 0.001D, "", Hints.alphaM);
            AddEngineParameter("ВЫСОТА_БАШМАКА", "hpb", 0D, 0.0d, 10.0D, 0.01D, "см", Hints.hpb);
            AddEngineParameter("ОБМОТОЧНЫЙ_КОЭФФИЦИЕНТ", "k0", 0.945D, 0.8D, 1D, 0.01D, "", Hints.k0);
            AddEngineParameter("КОЭФФИЦИЕНТ_ДИАМЕТРА_ВТУЛКИ", "kdvt", 0.44D, 0.2D, 0.9D, 0.01D, "", Hints.kdvt);
            AddEngineParameter("СООТНОШЕНИЕ_ШИРИНЫ_БАШМАКА_К_ПОЛЮСУ", "kpb", 0.993D, 0.1D, 1D, 0.01D, "", Hints.kpb);

            //статор
            AddEngineParameter("ЧИСЛО_ПАЗОВ_НА_ПОЛЮС_И_ФАЗУ", "q", 1.5D, 0.5D, 3.0D, 0.5D, "", Hints.q);
            AddEngineParameter("ЧИСЛО_ПАРАЛЛЕЛЬНЫХ_ВЕТВЕЙ_ОБМОТКИ", "a1", 1D, 1D, 10D, 1.0D, "", Hints.a1);
            AddEngineParameter("ЧИСЛО_ПРОВОДНИКОВ_В_ПАРАЛЛЕЛЬНЫХ_ВЕТВЯХ", "a2", 8D, 1D, 50D, 1.0D, "", Hints.a2);
            AddEngineParameter("ДОПУСТИМАЯ_ПЛОТНОСТЬ_ТОКА_В_ВЕТВИ", "Ja", 575D, 100D, 1000D, 1D, "А/см2", Hints.Ja);
            AddEngineParameter("КОЭФФИЦИЕНТ_ДИАМЕТРА_ИЗОЛИРОВАННОГО_ПРОВОДА", "kiz", 1.08D, 1D, 2D, 0.01D, "", Hints.kiz);
            AddEngineParameter("КОЭФФИЦИЕНТ_ЗАПОЛНЕНИЯ_ПАЗА", "kzp", 0.53D, 0.50D, 0.8D, 0.01D, "", Hints.kzp);
            AddEngineParameter("ИНДУКЦИЯ_ЗУБЦА", "Bz", 1.14D, 1D, 1.2D, 0.01D, "Вб/м2", Hints.Bz);

            AddEngineParameter("КОЭФФИЦИЕНТ_ЗАПОЛНЕНИЯ_ПАКЕТА_СТАТОРА", "kzs", 0.95D, 0.5d, 1D, 0.01D, "", Hints.kzs);
            AddEngineParameter("ДОПУСТИМАЯ_ИНДУКЦИЯ_СПИНКИ_СТАТОРА", "Bja", 1.5D, 1D, 1.7D, 0.1D, "Вб/м2", Hints.Bja);
            AddEngineParameter("ВЫСОТА_ШЛИЦА", "hsh", 0.07D, 0.0D, 1D, 0.01D, "см", Hints.hsh);
            AddEngineParameter("ВЫСОТА_КЛИНА", "hk", 0.07D, 0.0D, 1D, 0.01D, "см", Hints.hk);
            AddEngineParameter("СООТНОШЕНИЕ_ШИРИНЫ_ШЛИЦА_К_ПАЗУ", "ksh", 0.5D, 0.0D, 1D, 0.01D, "", Hints.ksh);

            //регуляторы
            AddEngineParameter("ПЛОТНОСТЬ_МАТЕРИАЛА_РОТОРА", "roSt", 7900D, 1000d, 20000D, 10D, "кг/см3", Hints.roSt);
            AddEngineParameter("КОЭФФИЦИЕНТ_ПЕРЕДАЧИ_ДАТЧИКА_СКОРОСТИ", "kds", 1D, 0.0001d, 1D, 0.0001D, "", Hints.kds);
            AddEngineParameter("КОЭФФИЦИЕНТ_УСИЛЕНИЯ_ПРЕОБРАЗОВАТЕЛЯ_ЧАСТОТЫ", "kpch", 1D, 0.0001d, 1D, 0.0001D, "", Hints.kpch);
            AddEngineParameter("КОЭФФИЦИЕНТ_ПЕРЕДАЧИ_ДАТЧИКА_ТОКА", "kdt", 1D, 0.01d, 1D, 0.0001D, "", Hints.kdt);
            AddEngineParameter("ПОСТОЯННАЯ_ВРЕМЕНИ_КОНТУРА_ТОКА", "Tkt", 0.01D, 0.0001d, 1D, 0.0001D, "", Hints.Tkt);
            AddEngineParameter("ПОСТОЯННАЯ_ВРЕМЕНИ_ФИЛЬТРА_СКОРОСТИ", "Tfs", 0.0001D, 0.0001d, 1D, 0.0001D, "", Hints.Tfs);
            AddEngineParameter("ПРЕВЫШЕНИЕ_ТЕМПЕРАТУРЫ_ОБМОТКИ_СТАТОРА", "deltaT", 30D, 0d, 50D, 1D, "", Hints.deltaT);
            AddEngineParameter("КОЭФФИЦИЕНТ_ДЛИНЫ_ЛОБОВОЙ_ЧАСТИ_ПОЛУВИТКА", "klv", 1.7D, 1.6d, 1.8D, 0.01D, "", Hints.klv);

        }

        void SolveStepMain()
        {
            AddEngineParameter("НОМИНАЛЬНАЯ_ЧАСТОТА_ВРАЩЕНИЯ", "nn",
                (60D * GetParam("f").Value / GetParam("p").Value), "Об/мин", Hints.nn);

            AddEngineParameter("УГЛОВАЯ_СКОРОСТЬ_ВРАЩЕНИЯ", "wn0",
                (2.0D * GetParam("nn").Value * Math.PI / 60.0D), "Рад/с", Hints.wn0);

            AddEngineParameter("НОМИНАЛЬНАЯ_МОЩНОСТЬ", "Pn",
                (GetParam("Mn").Value * GetParam("wn0").Value * GetParam("nu").Value * Math.Cos(GetParam("phi").Value) / GetParam("alpha1").Value), "Вт", Hints.Pn);

            AddEngineParameter("НОМИНАЛЬНОЕ_ФАЗНОЕ_НАПРЯЖЕНИЕ", "Ufn",
                (GetParam("Pn").Value / (GetParam("m").Value * GetParam("Ifn").Value * Math.Cos(GetParam("phi").Value) * GetParam("nu").Value)), "В", Hints.Ufn);

            AddEngineParameter("ИНДУКЦИЯ_ПРИ_ХОЛОСТОМ_ХОДЕ", "Bm0",
                (GetParam("kb").Value * GetParam("Br").Value), "Вб/м2", Hints.Bm0);

            AddEngineParameter("НАПРЯЖЕННОСТЬ_МАГНИТНОГО_ПОЛЯ", "Hmk",
                (GetParam("kh").Value * GetParam("Hc").Value), "А/м", Hints.Hmk);

        }

        void SolveStepRotor()
        {
            AddEngineParameter("ОБЪЕМ_МАГНИТА", "Vm",
                (
                    (
                    0.9D
                    * GetParam("Pn").Value
                    * GetParam("alpha1").Value
                    * GetParam("sigma0").Value
                    * GetParam("kad").Value
                    * GetParam("kud").Value
                    * GetParam("kkz").Value * GetParam("kkz").Value
                    )
                    /
                    (
                    4.0D
                    * GetParam("kf").Value
                    * GetParam("f").Value
                    * GetParam("Bm0").Value
                    * GetParam("Hmk").Value
                    * (GetParam("kkz").Value - 1)
                    )
                    * 1000000
                )
                , "см3", Hints.Vm);

            AddEngineParameter("ДИАМЕТР_МАГНИТА", "Dm", Math.Pow(
                (
                    (
                    4.0D
                    * GetParam("Vm").Value
                    * GetParam("kzap").Value
                    )
                    /
                    (
                    Math.PI
                    * GetParam("lambdaM").Value
                    * GetParam("kzm").Value
                    )
                ), 1.0D / 3.0D), "см", Hints.Dm);

            AddEngineParameter("ДЛИНА_МАГНИТА", "Lm",
                (GetParam("lambdaM").Value * GetParam("Dm").Value), "см", Hints.Lm);




            AddEngineParameter("ДЛИНА_РОТОРА", "Lr",
                (GetParam("Lm").Value), "см", Hints.Lr);

            AddEngineParameter("ДИАМЕТР_РОТОРА", "Dr",
                (GetParam("Dm").Value + 2 * GetParam("hpb").Value), "см", Hints.Dr);

            AddEngineParameter("ВЫСОТА_ВОЗДУШНОГО_ЗАЗОРА", "delta",
                (0.01 + 0.03 * Math.Pow(GetParam("Vm").Value, 1d / 3d)), "см", Hints.delta);

            AddEngineParameter("ДЛИНА_СРЕДНЕЙ_МАГНИТНОЙ_ЛИНИИ", "lm",
                (
                (1 + Math.PI / 8.0D)
                * GetParam("alphaM").Value
                * Math.PI
                * GetParam("Dm").Value
                / (2 * GetParam("p").Value)
                ), "см", Hints.lm);


            AddEngineParameter("ПОЛЮСНОЕ_ДЕЛЕНИЕ_РОТОРА_ПО_МАГНИТУ", "tau",
                (GetParam("Dm").Value * Math.PI / (2 * GetParam("p").Value)), "см", Hints.tau);

            //===============
            AddEngineParameter("ДЛИНА_ДУГИ_ПОЛЮСНОГО_БАШМАКА", "bpb",
                (GetParam("kpb").Value * GetParam("tau").Value), "см", Hints.bpb);


            AddEngineParameter("ШИРИНА_ПОЛЮСНОЙ_ДУГИ", "bp",
                (GetParam("tau").Value * GetParam("alphaM").Value), "см", Hints.bp);

            AddEngineParameter("ШИРИНА_МАГНИТА", "bm",
                (
                GetParam("Dm").Value *
                Math.Sin(Math.PI * GetParam("alphaM").Value / (2 * GetParam("p").Value))
                ), "см", Hints.bm);

            AddEngineParameter("ПЛОЩАДЬ_СЕЧЕНИЯ_МАГНИТА", "Sm",
                (GetParam("Vm").Value / (2 * GetParam("p").Value * GetParam("Lm").Value)), "см2", Hints.Sm);

            AddEngineParameter("ЛИНЕЙНАЯ_НАГРУЗКА_НА_МАГНИТ", "A", (
                GetParam("Hmk").Value * GetParam("lm").Value
                / (
                Math.PI * GetParam("Dm").Value * GetParam("kkz").Value * GetParam("k0").Value
                * 0.9d * GetParam("alpha1").Value * GetParam("kad").Value
                )), "Дж", Hints.A);

            AddEngineParameter("ВНУТРЕННИЙ_ДИАМЕТР_МАГНИТА", "dm",
                (
                    2 * Math.Sqrt(
                    GetParam("Dm").Value * GetParam("Dm").Value / 4 -
                    2 * GetParam("p").Value * GetParam("Sm").Value /
                    (GetParam("alphaM").Value * Math.PI)
                )), "см", Hints.dm);

            AddEngineParameter("ДИАМЕТР_ВТУЛКИ", "dvt",
                 (GetParam("kdvt").Value * GetParam("dm").Value), "см", Hints.dvt);


            AddEngineParameter("ВЫСОТА_МАГНИТА", "hm",
                (
                (GetParam("Dm").Value - GetParam("dm").Value) / 2
                ), "см", Hints.hm);



        }

        void SolveStepStator()
        {
            AddEngineParameter("ПЛОЩАДЬ_ПОВЕРХНОСТИ_МАГНИТА", "Smp",
                (GetParam("bp").Value * GetParam("Lm").Value), "см2", Hints.Smp);

            AddEngineParameter("МАГНИТНЫЙ_ПОТОК_В_ЗАЗОРЕ", "PHIdelta0", (
                GetParam("Bm0").Value *
                GetParam("Smp").Value
                * 0.0001
                / GetParam("sigma0").Value), "Вб", Hints.PHIdelta0);

            AddEngineParameter("НОМИНАЛЬНАЯ_ЭДС_ХОЛОСТОГО_ХОДА", "E0",
                (
                GetParam("Ufn").Value
                * Math.Sqrt(2 * (1 + Math.Sin(GetParam("phi").Value)))
            ), "В", Hints.E0);

            AddEngineParameter("ЧИСЛО_ВИТКОВ_В_ФАЗЕ", "wf", (
                GetParam("E0").Value
                / (4.0D * GetParam("kf").Value *
                GetParam("k0").Value *
                GetParam("f").Value *
                GetParam("PHIdelta0").Value))
                , "", Hints.wf);

            AddEngineParameter("ЧИСЛО_ПАЗОВ_СТАТОРА", "z",
                2.0D * GetParam("q").Value *
                GetParam("p").Value *
                GetParam("m").Value, "", Hints.z);

            AddEngineParameter("ЧИСЛО_ПРОВОДНИКОВ_В_ПАЗУ", "Np",
                (2 * GetParam("m").Value *
                GetParam("wf").Value *
                GetParam("a1").Value *
                GetParam("a2").Value
                / GetParam("z").Value), "", Hints.Np);

            AddEngineParameter("ПЛОЩАДЬ_СЕЧЕНИЯ_ПРОВОДА", "Spr",
                (GetParam("Ifn").Value /
                (GetParam("Ja").Value *
                GetParam("a1").Value *
                GetParam("a2").Value)), "см2", Hints.Spr);

            //=============

            AddEngineParameter("ДИАМЕТР_ПРОВОДА", "dpr",
                (2 * Math.Sqrt(GetParam("Spr").Value / Math.PI)), "см", Hints.dpr);

            AddEngineParameter("ДИАМЕТР_ПРОВОДА_С_ИЗОЛЯЦИЕЙ", "diz",
                (GetParam("kiz").Value * GetParam("dpr").Value), "см", Hints.diz);

            AddEngineParameter("ПЛОЩАДЬ_СЕЧЕНИЯ_ПРОВОДА_С_ИЗОЛЯЦИЕЙ", "Siz",
                (Math.PI * Math.Pow(GetParam("diz").Value, 2d) / 4), "см2", Hints.Siz);


            AddEngineParameter("ПЛОЩАДЬ_ПАЗА", "Sp",
                (GetParam("Np").Value * GetParam("Siz").Value /
                GetParam("kzp").Value), "см2", Hints.Sp);


            AddEngineParameter("ВНУТРЕННИЙ_ДИАМЕТР_СТАТОРА", "D",
                (GetParam("Dr").Value + 2.0D * GetParam("delta").Value), "см", Hints.D);

            AddEngineParameter("ЗУБЦОВОЕ_ДЕЛЕНИЕ_СТАТОРА", "tz",
                Math.PI * GetParam("D").Value / GetParam("z").Value, "см", Hints.tz);


            AddEngineParameter("ИНДУКЦИЯ_В_ЗАЗОРЕ", "Bdelta0",
                (GetParam("PHIdelta0").Value /
                (GetParam("alphaM").Value *
                GetParam("tau").Value *
                GetParam("Lm").Value *
                0.0001)), "Вб/м2", Hints.Bdelta0);

            AddEngineParameter("ШИРИНА_ЗУБЦА", "bz",
                (GetParam("Bdelta0").Value * GetParam("tz").Value /
                (GetParam("Bz").Value * GetParam("kzs").Value)), "см", Hints.bz);

            //=======================

            AddEngineParameter("ШИРИНА_ПАЗА_МИН", "bpmin",
                ((Math.PI * (GetParam("D").Value + 2 * GetParam("hk").Value) / GetParam("z").Value) -
                GetParam("bz").Value), "см", Hints.bpmin);


            AddEngineParameter("ВЫСОТА_СПИНКИ_СТАТОРА", "hja",
                (GetParam("PHIdelta0").Value * 10000 /
                (2.0D * GetParam("lm").Value *
                GetParam("kzs").Value * GetParam("Bja").Value)), "см", Hints.hja);



            AddEngineParameter("ШИРИНА_ПАЗА_СРЕД", "bpsr",
                (
                Math.PI * GetParam("D").Value - GetParam("z").Value * GetParam("bz").Value +
                    Math.Sqrt(
                        Math.Pow(Math.PI * GetParam("D").Value - GetParam("z").Value * GetParam("bz").Value, 2) +
                    4 * GetParam("z").Value * GetParam("Sp").Value * Math.PI)
                )
                / (2 * GetParam("z").Value), "см", Hints.bpsr);


            AddEngineParameter("ШИРИНА_ПАЗА_МАКС", "bpmax",
                2 * GetParam("bpsr").Value - GetParam("bpmin").Value, "см", Hints.bpmax);

            AddEngineParameter("ВЫСОТА_ПАЗА", "hp",
                GetParam("Sp").Value / GetParam("bpsr").Value, "см", Hints.hp);

            AddEngineParameter("ДИАМЕТР_СТАТОРА_С_ПАЗАМИ", "Dpaz",
               2 * GetParam("hp").Value + 2 * GetParam("hk").Value + 2 * GetParam("hsh").Value + GetParam("D").Value, "см", Hints.Dpaz);

            AddEngineParameter("ВНЕШНИЙ_ДИАМЕТР_СТАТОРА", "Dstv",
               2 * GetParam("hja").Value + GetParam("Dpaz").Value, "см", Hints.Dstv);

            AddEngineParameter("ШИРИНА_ШЛИЦА", "bshl",
               GetParam("ksh").Value * GetParam("bpmin").Value, "см", Hints.bshl);

        }


        void SolveRegulators()
        {
            AddEngineParameter("ОБЪЕМ_РОТОРА", "Vr",
               GetParam("Lr").Value * Math.Pow((GetParam("Dm").Value / 2), 2) * Math.PI, "см3", Hints.Vr);

            AddEngineParameter("МАССА_РОТОРА", "mr",
               GetParam("Vr").Value * GetParam("roSt").Value * Math.Pow(10, -6), "кг", Hints.mr);

            AddEngineParameter("МОМЕНТ_ИНЕРЦИИ_РОТОРА", "Jr",
               GetParam("mr").Value / 12 * (3 * Math.Pow(GetParam("Dm").Value / 2, 2)
               + Math.Pow(GetParam("Lm").Value, 2)) * Math.Pow(10, -4), "кг/м2", Hints.Jr);

            AddEngineParameter("ПОЛНОЕ_ПОТОКОСЦЕПЛЕНИЕ", "psi",
               2 * GetParam("p").Value * GetParam("PHIdelta0").Value, "Вб", Hints.psi);

            AddEngineParameter("ИНДУКТИВНОСТЬ_СТАТОРА", "Lst",
               GetParam("psi").Value / GetParam("Ifn").Value, "Гн", Hints.Lst);

            AddEngineParameter("ДЛИНА_ЛОБОВОЙ_ЧАСТИ_ПОЛУВИТКА", "ls",
               GetParam("klv").Value * GetParam("tz").Value, "см", Hints.ls);

            AddEngineParameter("СРЕДНЯЯ_ДЛИНА_ВИТКА_ОБМОТКИ_СТАТОРА", "lsr",
               2 * (GetParam("Lm").Value + GetParam("ls").Value), "см", Hints.lsr);

            //======================

            AddEngineParameter("СОПРОТИВЛЕНИЕ_ОБМОТКИ_СТАТОРА", "Rs",
                (GetParam("m").Value * GetParam("wf").Value * GetParam("lsr").Value * 0.01) /
               (GetParam("Spr").Value * 100 * 5700)
               * (1 + 0.004 * GetParam("deltaT").Value), "Ом", Hints.Rs);


            AddEngineParameter("ПРОПОРЦИОНАЛЬНЫЙ_КОЭФФИЦИЕНТ_ТОКА", "Ki",
               GetParam("Lst").Value /
               (2 * GetParam("kpch").Value * GetParam("kdt").Value * GetParam("Tkt").Value), "", Hints.Ki);

            AddEngineParameter("ИНТЕГРАЛЬНЫЙ_КОЭФФИЦИЕНТ_ТОКА", "Ti",
               GetParam("Rs").Value /
               (2 * GetParam("kpch").Value * GetParam("kdt").Value * GetParam("Tkt").Value), "", Hints.Ti);


            AddEngineParameter("ПОСТОЯННАЯ_ВРЕМЕНИ_КОНТУРА_СКОРОСТИ", "Tks",
               2 * GetParam("Tkt").Value + GetParam("Tfs").Value, "", Hints.Tks);


            AddEngineParameter("ПРОПОРЦИОНАЛЬНЫЙ_КОЭФФИЦИЕНТ_СКОРОСТИ", "Kw",
               GetParam("kdt").Value * GetParam("Jr").Value /
               (3 * GetParam("Tks").Value * GetParam("psi").Value * GetParam("p").Value * GetParam("kds").Value), "", Hints.Kw);

            AddEngineParameter("ИНТЕГРАЛЬНЫЙ_КОЭФФИЦИЕНТ_СКОРОСТИ", "Tw",
               GetParam("kdt").Value * GetParam("Jr").Value /
               (12 * GetParam("Tks").Value * GetParam("Tks").Value * GetParam("psi").Value * GetParam("p").Value * GetParam("kds").Value), "", Hints.Tw);


        }


    }
}
