using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ELCUT;
using System.IO;
using EngineBuilder.Entities;

namespace EngineBuilder
{
    public delegate void ProgressDelegate(string mes, bool isCaption);

    class Elcut
    {

        public event ProgressDelegate eventProgress;
        string workDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\EngineBuilder_WORK\\";


        private void OnEventProgress(string mes, bool isCaption)
        {
            if (eventProgress != null)
            {
                eventProgress.Invoke(mes, isCaption);
            }
        }

        Application application; //Приложение ELCUT
        Problem problem; //Наша задача или проблема, которую решаем
        Model model; //Наша модель, предназначенная для отрисовки геометрии
        //DataDoc dataDoc; //Документ, хранящий физические свойства
        Shapes shapes;
        //ShapeRange shapesRange; //Временная коллекция геометрических объектов 
        Engine engine;




        public Elcut(Engine engine)
        {
            this.engine = engine;

            if (!Directory.Exists(workDirectory))
                Directory.CreateDirectory(workDirectory);
        }

        public void Run()
        {
            try
            {
                application = new Application();

                if (application.ActiveProblem != null)
                {
                    application.ActiveProblem.Model.Close();
                    application.ActiveProblem.DataDoc.Close();
                    application.ActiveProblem.Close();
                }

                File.Delete(workDirectory + "MODEL.mod");
                File.Delete(workDirectory + "DATA.dms");
                File.Delete(workDirectory + "PROBLEM.pbm");


                OnEventProgress("Постановка задачи", true);

                problem = (Problem)application.Problems.Add(); //Создание новой задачи
                problem.ProblemType = QfProblemTypes.qfMagnetostatics; //Установка типа задачи - магнитостатика
                problem.Class = QfProblemClasses.qfPlaneParallel; //Установка класса задачи - 
                problem.LengthUnits = QfLengthUnits.qfCentimeters; //Единица длины
                problem.Coordinates = QfCoordinates.qfCartesian; //Установка типа исп системы координат
                problem.set_ReferencedFile(QfProblemFiles.qfModelFile, workDirectory + "MODEL.mod"); //Файлы, используемые в задаче (проблеме)
                problem.set_ReferencedFile(QfProblemFiles.qfDataFile, workDirectory + "DATA.dms");
                problem.SaveAs(workDirectory + "PROBLEM.pbm");
                problem.ZLength = engine.GetEngineParameter("ДЛИНА_РОТОРА").Value;

                problem.Activate();

                model = (Model)application.Models.Add();
                model.SaveAs(workDirectory + "MODEL.mod");

                //зум модели
                (model.Windows.Item(1) as ModelWindow).Zoom(application.PointXY(0.25, 0.25), application.PointXY(-0.25, -0.25));

                //геометрия
                BuildGeometry();

                //магнитные свойства
                BindMagneticProperties();

                //============
                OnEventProgress("Построение сетки", true);
                OnEventProgress("{ Расчет сетки }", false);

                shapes.Select();
                application.ActiveProblem.Model.Shapes.BuildMesh();
                //=============

                OnEventProgress("Сохранение модели", true);

                //сохраняем
                model.Save();
                problem.DataDoc.Save();
                problem.Save();

                //активируем окно с моделью
                model.Activate();

                OnEventProgress("Построение модели завершено", true);
            }
            catch (Exception e)
            {
                OnEventProgress("Ошибка", true);
                OnEventProgress(e.Message, false);
            }
        }


        private void BuildGeometry()
        {
            shapes = model.Shapes;
            Point pointSpacing = application.PointXY(0.001D, 0.001D);
            model.Grid.Spacing = pointSpacing;

            //**************************************
            OnEventProgress("Построение основных окружностей", true);
            OnEventProgress("{ Основные окружности }", false);


            //внешняя окружность ротора
            double Rrotor = engine.GetEngineParameter("ДИАМЕТР_РОТОРА").Value / 2.0D;
            OnEventProgress("Внешний радиус ротора = " + Rrotor, false);
            Point pointDrotor1 = application.PointXY(-Rrotor, 0.0D);
            Point pointDrotor2 = application.PointXY(Rrotor, 0.0D);
            shapes.AddEdge(pointDrotor1, pointDrotor2, Math.PI);
            shapes.AddEdge(pointDrotor1, pointDrotor2, -Math.PI);

            //втулка
            double Rvtylka = engine.GetEngineParameter("ДИАМЕТР_ВТУЛКИ").Value / 2.0D;
            OnEventProgress("Радиус втулки = " + Rvtylka, false);
            Point pointDvtylka1 = application.PointXY(-Rvtylka, 0.0D);
            Point pointDvtylka2 = application.PointXY(Rvtylka, 0.0D);
            shapes.AddEdge(pointDvtylka1, pointDvtylka2, Math.PI);
            shapes.AddEdge(pointDvtylka1, pointDvtylka2, -Math.PI);



            double vneshRstator = engine.GetEngineParameter("ВНЕШНИЙ_ДИАМЕТР_СТАТОРА").Value / 2.0D;
            double vnutRstator = engine.GetEngineParameter("ВНУТРЕННИЙ_ДИАМЕТР_СТАТОРА").Value / 2.0D;

            if (!engine.onlyRotor)
            {
                //внешняя окружность статора
                OnEventProgress("Внешний радиус статора = " + vneshRstator, false);
                Point pointDstator1 = application.PointXY(-vneshRstator, 0.0D);
                Point pointDstator2 = application.PointXY(vneshRstator, 0.0D);
                shapes.AddEdge(pointDstator1, pointDstator2, Math.PI).Label = "НУЛЬ";
                shapes.AddEdge(pointDstator1, pointDstator2, -Math.PI).Label = "НУЛЬ";

                LabelEdgeMS content2 = problem.Labels[QfShapes.qfEdge].Item("НУЛЬ").Content;
                content2.Dirichlet = 0;
                problem.Labels[QfShapes.qfEdge].Item("НУЛЬ").Content = content2;

                //внутренняя окружность статора
                OnEventProgress("Внутренний радиус статора = " + vnutRstator, false);
                Point pointVnDstator1 = application.PointXY(-vnutRstator, 0.0D);
                Point pointVnDstator2 = application.PointXY(vnutRstator, 0.0D);
                shapes.AddEdge(pointVnDstator1, pointVnDstator2, Math.PI);
                shapes.AddEdge(pointVnDstator1, pointVnDstator2, -Math.PI);
            }

            //**********************************************************
            if (!engine.onlyRotor)
            {
                //называем статор
                Point statorP = application.PointXY(vneshRstator - 0.0001, 0.0D);
                shapes.Blocks.get_Nearest(statorP).Blocks.Item(1).Label = "СТАТОР";

                //называем зазор
                Point zazorP = application.PointXY(vnutRstator - 0.0001, 0.0D);
                shapes.Blocks.get_Nearest(zazorP).Blocks.Item(1).Label = "ВОЗДУШНЫЙ_ЗАЗОР";
            }

            //называем втулку
            shapes.Blocks.get_Nearest(application.PointXY(0D, 0.0D)).Blocks.Item(1).Label = "ВТУЛКА";

            //называем ротор
            shapes.Blocks.get_Nearest(application.PointXY(Rvtylka + 0.0001, 0.0D)).Blocks.Item(1).Label = "РОТОР";


            //**********************************************************

            //контур
            double Rcontur = (Rrotor + vnutRstator) / 2;
            OnEventProgress("Радиус контура = " + Rcontur, false);
            Point pointRcontur1 = application.PointXY(-Rcontur, 0.0D);
            Point pointRcontur2 = application.PointXY(Rcontur, 0.0D);
            shapes.AddEdge(pointRcontur1, pointRcontur2, Math.PI).Label = "КОНТУР";
            shapes.AddEdge(pointRcontur1, pointRcontur2, -Math.PI).Label = "КОНТУР";

            LabelEdgeMS content = problem.Labels[QfShapes.qfEdge].Item("КОНТУР").Content;
            problem.Labels[QfShapes.qfEdge].Item("КОНТУР").Content = content;

            //**********************************************************
            if (!engine.onlyRotor)
            {
                OnEventProgress("Построение пазов", true);
                OnEventProgress("{ Отрисовка пазов }", false);

                // Шаг отрисовки пазов
                double step = 2 * Math.PI / engine.GetEngineParameter("ЧИСЛО_ПАЗОВ_СТАТОРА").Value;
                OnEventProgress("Шаг отрисовки = " + step, false);
                OnEventProgress("Число пазов = " + engine.GetEngineParameter("ЧИСЛО_ПАЗОВ_СТАТОРА").Value, false);

                DrawPaz(); //отрисовка пазов
            }
            //**********************************************************
            OnEventProgress("Построение башмаков", true);
            OnEventProgress("{ Отрисовка башмаков }", false);

            DrawBashmak(); //отрисовка башмаков

            //**********************************************************
            OnEventProgress("Построение магнитов", true);
            OnEventProgress("{ Отрисовка магнитов }", false);

            DrawMagnet(); //отрисовка магнитов

            //**********************************************************


            OnEventProgress("Геометрическое построение завершено", true);
            OnEventProgress("{ Геометрическое построение завершено }", false);
        }

        private void DrawPaz()
        {
            Point[] mPP = new Point[16];

            #region Описание массива
            /*
            mPP[0] - ШЛИЦ удаленная точка (коронка зубца)
            mPP[1] - ШЛИЦ удаленная точка
            mPP[2] - ШЛИЦ ближайшая точка
            mPP[3] - ШЛИЦ ближайшая точка
             * 
            mPP[4] - КЛИН удаленная точка (переход)
            mPP[5] - КЛИН удаленная точка
            mPP[6] - КЛИН ближайшая точка
            mPP[7] - КЛИН ближайшая точка
             * 
            mPP[8] - ПАЗ удаленная точка (паз)
            mPP[9] - ПАЗ удаленная точка
            mPP[10] - ПАЗ ближайшая точка
            mPP[11] - ПАЗ ближайшая точка
             * 
            mPP[12] - базовая точка центра блока ШЛИЦА
            mPP[13] - базовая точка центра блока КЛИНА
            mPP[14] - базовая точка центра блока ПАЗА
            *
            */
            #endregion

            double vnRstator = engine.GetEngineParameter("ВНУТРЕННИЙ_ДИАМЕТР_СТАТОРА").Value / 2.0D;

            //Шлиц
            OnEventProgress("{ Шлиц }", false);
            double hShlitc = engine.GetEngineParameter("ВЫСОТА_ШЛИЦА").Value;
            OnEventProgress("Высота шлица = " + hShlitc, false);


            //шлиц - удаленные точки
            double angleShlitc = (engine.GetEngineParameter("ШИРИНА_ШЛИЦА").Value) /
                (vnRstator + hShlitc);
            mPP[0] = application.PointRA(vnRstator + hShlitc, -angleShlitc / 2.0D);
            mPP[1] = application.PointRA(vnRstator + hShlitc, angleShlitc / 2.0D);
            InfoPoint("Удаленная точка 1 шлица:", mPP[0]);
            InfoPoint("Удаленная точка 2 шлица:", mPP[1]);

            //шлиц - ближайшие точки
            double angleShlitc2 = (engine.GetEngineParameter("ШИРИНА_ШЛИЦА").Value) /
                (vnRstator);
            mPP[2] = application.PointRA(vnRstator, -angleShlitc2 / 2.0D);
            mPP[3] = application.PointRA(vnRstator, angleShlitc2 / 2.0D);
            InfoPoint("Ближайшая точка 1 шлица:", mPP[2]);
            InfoPoint("Ближайшая точка 2 шлица:", mPP[3]);

            //высота клина
            OnEventProgress("{ Клин }", false);
            double hKlin = engine.GetEngineParameter("ВЫСОТА_КЛИНА").Value;
            OnEventProgress("Высота клина = " + hKlin, false);

            //Удаленные точки клина
            double shirinaPazaMin = engine.GetEngineParameter("ШИРИНА_ПАЗА_МИН").Value;
            double rDoPaza = vnRstator + hShlitc + hKlin;
            double angleKlin = shirinaPazaMin / rDoPaza;
            mPP[4] = application.PointRA(rDoPaza, -angleKlin / 2.0D);
            mPP[5] = application.PointRA(rDoPaza, angleKlin / 2.0D);
            InfoPoint("Удаленная точка 1 клина:", mPP[4]);
            InfoPoint("Удаленная точка 2 клина:", mPP[5]);

            //ближние точки клина = удаленные точки шлица
            mPP[6] = application.PointRA(mPP[0].R, mPP[0].Phi);
            mPP[7] = application.PointRA(mPP[1].R, mPP[1].Phi);
            InfoPoint("Ближайшая точка 1 клина:", mPP[6]);
            InfoPoint("Ближайшая точка 2 клина:", mPP[7]);

            //дальние точки паза
            OnEventProgress("{ Паз }", false);
            double hPaz = engine.GetEngineParameter("ВЫСОТА_ПАЗА").Value;
            OnEventProgress("Высота паза = " + hPaz, false);

            double shirinaPazaMax = engine.GetEngineParameter("ШИРИНА_ПАЗА_МАКС").Value;
            double rPaz = rDoPaza + hPaz;
            double anglePaz = shirinaPazaMax / rPaz;
            mPP[8] = application.PointRA(rPaz, -anglePaz / 2.0D);
            mPP[9] = application.PointRA(rPaz, anglePaz / 2.0D);
            InfoPoint("Удаленная точка 1 паза:", mPP[8]);
            InfoPoint("Удаленная точка 2 паза:", mPP[9]);

            //ближайшие точки паза = удаленные точки клина
            mPP[10] = application.PointRA(mPP[4].R, mPP[4].Phi);
            mPP[11] = application.PointRA(mPP[5].R, mPP[5].Phi);
            InfoPoint("Ближайшая точка 1 паза:", mPP[10]);
            InfoPoint("Ближайшая точка 2 паза:", mPP[11]);

            //отрисовка линий и вспомогательные центральные точки
            double step = 2 * Math.PI / engine.GetEngineParameter("ЧИСЛО_ПАЗОВ_СТАТОРА").Value;
            double countPaz = engine.GetEngineParameter("ЧИСЛО_ПАЗОВ_СТАТОРА").Value;
            mPP[12] = DrawPazLines(mPP, 0, "ШЛИЦЫ", step, countPaz, angleShlitc);
            mPP[13] = DrawPazLines(mPP, 4, "КЛИНЫ", step, countPaz, angleKlin);
            mPP[14] = DrawPazLines(mPP, 8, "ПАЗЫ", step, countPaz, anglePaz);

            //линия, делящая паз на 2 части
            Point one = application.PointRA((mPP[8].R + mPP[9].R) / 2, (mPP[8].Phi + mPP[9].Phi) / 2);
            Point two = application.PointRA((mPP[10].R + mPP[11].R) / 2, (mPP[10].Phi + mPP[11].Phi) / 2);
            ShapeRange shape = shapes.AddEdge(one, two);

            shape.Duplicate(QfTransformType.qfRotation,
                application.PointXY(0.0D, 0.0D),
                step,
                (int)countPaz);

            //именуем каждую часть паза как отдельный паз
            Point basePoint1 = application.PointXY(mPP[14].R, mPP[14].Phi - 0.0001);
            Point basePoint2 = application.PointXY(mPP[14].R, mPP[14].Phi + 0.0001);

            for (int i = 0; i < (int)countPaz; i++)
            {
                ShapeRange sr1 = shapes.Blocks.get_Nearest(basePoint1);
                Block currentBlock1 = (Block)sr1.Item(1);
                currentBlock1.Select();
                currentBlock1.Label = "ПАЗ_" + Convert.ToString(i * 2 + 1);
                basePoint1.Phi -= step; //нумеруем в другую сторону, потому что модель не работает

                ShapeRange sr2 = shapes.Blocks.get_Nearest(basePoint2);
                Block currentBlock2 = (Block)sr2.Item(1);
                currentBlock2.Select();
                currentBlock2.Label = "ПАЗ_" + Convert.ToString(i * 2 + 2);
                basePoint2.Phi -= step;
            }
        }

        private Point DrawPazLines(Point[] mPP, int index, string name, double step, double countPaz, double angle)
        {
            //линии
            shapes.AddEdge(mPP[index], mPP[index + 1], angle);
            shapes.AddEdge(mPP[index + 2], mPP[index]);
            shapes.AddEdge(mPP[index + 3], mPP[index + 1]);
            //shapes.AddEdge(mPP[index + 2], mPP[index + 3]);

            //вспомогательная точка
            Point basePoint = application.PointRA((mPP[index].X + mPP[index + 2].X) / 2.0D, 0.0D);

            //отрисовка
            ShapeRange sr = shapes.Blocks.get_Nearest(basePoint);
            Block block = (Block)sr.Item(1);
            block.Label = name;
            block.Select();
            Point center = application.PointXY(0.0D, 0.0D);
            sr = model.Selection;
            sr.Duplicate(QfTransformType.qfRotation,
                center,
                step,
                (int)countPaz);

            return basePoint;
        }

        private void DrawMagnet()
        {
            Point[] mPM = new Point[6];

            #region Описание массива
            /* Описание массива
            mPM[0] - ближайшие точки магнита
            mPM[1] - ближайшие точки магнита
             * 
            mPM[2] - удаленная точка для магнита
            mPM[3] - удаленная точка для магнита
             * 
            mPM[4] - базисная точка для магнита
             *
            mPM[5] - точка для соединения линии между магнитами
             */
            #endregion

            double outerRmagn = engine.GetEngineParameter("ДИАМЕТР_МАГНИТА").Value / 2.0D;
            int countMagn = (int)engine.GetEngineParameter("ЧИСЛО_ПАР_ПОЛЮСОВ").Value * 2;

            OnEventProgress("{ Магнит }", false);
            double hMagn = engine.GetEngineParameter("ВЫСОТА_МАГНИТА").Value;
            OnEventProgress("Высота магнита = " + hMagn, false);

            double angleMagn = engine.GetEngineParameter("ШИРИНА_ПОЛЮСНОЙ_ДУГИ").Value / outerRmagn;
            double innerRmagn = outerRmagn - hMagn;

            //ближние точки магнита
            mPM[0] = application.PointRA(innerRmagn, angleMagn / 2);
            mPM[1] = application.PointRA(innerRmagn, -angleMagn / 2);
            InfoPoint("Ближайшая точка 1 магнита:", mPM[0]);
            InfoPoint("Ближайшая точка 2 магнита:", mPM[1]);

            //удаленные точки магнита
            mPM[2] = application.PointRA(outerRmagn, angleMagn / 2);
            mPM[3] = application.PointRA(outerRmagn, -angleMagn / 2);
            InfoPoint("Удаленная точка 1 магнита:", mPM[2]);
            InfoPoint("Удаленная точка 2 магнита:", mPM[3]);

            //линии
            shapes.AddEdge(mPM[0], mPM[2]);
            shapes.AddEdge(mPM[1], mPM[3]);

            //внутренняя окружность магнитов
            Point pointDrotor1 = application.PointXY(-innerRmagn, 0.0D);
            Point pointDrotor2 = application.PointXY(innerRmagn, 0.0D);
            shapes.AddEdge(pointDrotor1, pointDrotor2, Math.PI);
            shapes.AddEdge(pointDrotor1, pointDrotor2, -Math.PI);

            //Базисная точка в центре
            mPM[4] = application.PointRA(hMagn / 2 + innerRmagn, 0.0D);

            //копируем магниты
            ShapeRange sr = shapes.Blocks.get_Nearest(mPM[4]);
            Block block = (Block)sr.Item(1);
            block.Select();
            sr = model.Selection;
            sr.Duplicate(QfTransformType.qfRotation,
                application.PointXY(0.0D, 0.0D),
                2 * Math.PI / countMagn,
                countMagn);

            //нумеруем магниты
            Point basePoint = mPM[4];
            for (int i = 0; i < countMagn; i++)
            {
                ShapeRange sr2 = shapes.Blocks.get_Nearest(basePoint);
                Block currentBlock = (Block)sr2.Item(1);
                currentBlock.Select();
                currentBlock.Label = "МАГНИТ_" + Convert.ToString(i + 1);
                basePoint.Phi += (2 * Math.PI / countMagn);
            }

            shapes.AddVertex(mPM[4]).Label = "УГОЛ";
            LabelVertexMS content = problem.Labels[QfShapes.qfVertex].Item("УГОЛ").Content;
            content.Dirichlet = 0;
            content.SetEmpty();
            problem.Labels[QfShapes.qfVertex].Item("УГОЛ").Content = content;

        }

        private void DrawBashmak()
        {

            Point[] mPB = new Point[5];

            #region Описание массива
            /* Описание массива
            mPM[0] - ближайшие точки башмака
            mPM[1] - ближайшие точки башмака
             * 
            mPM[2] - удаленная точка для башмака
            mPM[3] - удаленная точка для башмака
             * 
            mPM[4] - базисная точка для башмака
             */
            #endregion

            int countBashmak = (int)engine.GetEngineParameter("ЧИСЛО_ПАР_ПОЛЮСОВ").Value * 2;
            double Rmagn = engine.GetEngineParameter("ДИАМЕТР_МАГНИТА").Value / 2.0D;
            double Rbashm = engine.GetEngineParameter("ДИАМЕТР_РОТОРА").Value / 2d;

            if ((Rbashm - Rmagn) == 0) return;

            double angleBashmak = engine.GetEngineParameter("ДЛИНА_ДУГИ_ПОЛЮСНОГО_БАШМАКА").Value / Rmagn;

            OnEventProgress("{ Башмак }", false);
            OnEventProgress("Высота башмака = " + (Rbashm - Rmagn), false);

            //ближние точки
            mPB[0] = application.PointRA(Rmagn, angleBashmak / 2);
            mPB[1] = application.PointRA(Rmagn, -angleBashmak / 2);
            InfoPoint("Ближайшая точка 1 башмака:", mPB[0]);
            InfoPoint("Ближайшая точка 2 башмака:", mPB[1]);

            //дальние точки
            mPB[2] = application.PointRA(Rbashm, angleBashmak / 2);
            mPB[3] = application.PointRA(Rbashm, -angleBashmak / 2);
            InfoPoint("Удаленная точка 1 башмака:", mPB[2]);
            InfoPoint("Удаленная точка 2 башмака:", mPB[3]);

            //стороны башмака
            shapes.AddEdge(mPB[0], mPB[2]);
            shapes.AddEdge(mPB[1], mPB[3]);

            //внутренняя окружность башмака
            Point pointBashm1 = application.PointXY(Rmagn, 0.0D);
            Point pointBashm2 = application.PointXY(-Rmagn, 0.0D);
            shapes.AddEdge(pointBashm1, pointBashm2, Math.PI);
            shapes.AddEdge(pointBashm1, pointBashm2, -Math.PI);

            //вспомогательная точка
            mPB[4] = application.PointRA((mPB[0].R + mPB[2].R) / 2.0D, 0.0D);

            //копируем башмаки
            ShapeRange sr = shapes.Blocks.get_Nearest(mPB[4]);
            Block block = (Block)sr.Item(1);
            block.Select();
            block.Label = "БАШМАКИ";
            sr = model.Selection;
            sr.Duplicate(QfTransformType.qfRotation,
                application.PointXY(0.0D, 0.0D),
                2 * Math.PI / countBashmak,
                countBashmak);
        }


        private void BindMagneticProperties()
        {

            OnEventProgress("Распределение пазов по фазам", true);
            OnEventProgress("{ Распределение пазов по фазам }", false);

            if (!engine.onlyRotor)
                BindPhases();


            OnEventProgress("Определение магнитных свойств", true);
            OnEventProgress("{ Определение магнитных свойств }", false);

            OnEventProgress("Установка магнитной проницаемости", true);
            //устанавливаем для всех блоков магнитную проницаемость 1
            foreach (Label item in problem.Labels[QfShapes.qfBlock])
            {
                LabelBlockMS content = (LabelBlockMS)item.Content;
                content.Kxx = 1;
                content.Kyy = 1;
                item.Content = content;
            }


            OnEventProgress("Установка материала магнита", true);
            BindMagnete();

            OnEventProgress("Установка типа стали", true);
            //параметры стали для ротора
            Label labelRotor = problem.Labels[QfShapes.qfBlock].Item("РОТОР");
            LabelBlockMS content1 = (LabelBlockMS)labelRotor.Content;
            BindBH(content1, engine.rotorMaterial);
            labelRotor.Content = content1;

            //параметры стали для статора
            Label labelStator = problem.Labels[QfShapes.qfBlock].Item("СТАТОР");
            LabelBlockMS content2 = (LabelBlockMS)labelStator.Content;
            BindBH(content2, engine.statorMaterial);
            labelStator.Content = content2;

            //параметры стали для башмаков
            if (engine.GetEngineParameter("ВЫСОТА_БАШМАКА").Value > 0)
            {
                Label labelBashm = problem.Labels[QfShapes.qfBlock].Item("БАШМАКИ");
                LabelBlockMS content3 = (LabelBlockMS)labelBashm.Content;
                BindBH(content3, engine.bashmakMaterial);
                labelBashm.Content = content3;
            }



        }

        private void BindPhases()
        {
            //разбиваем пазы по фазам
            int halfPazCount = (int)engine.GetParam("z").Value*2;
            int phaseCount = (int)engine.GetParam("m").Value;

            //сколько полупазов на 1 фазу
            int q = (int)(engine.GetParam("q").Value / 0.5); 

            int phase = 1;
            int pazStart = 1;
            int paz = pazStart;

            do
            {
                for (int i = 0; i < q; i++)
                {
                    Shape pazBlock = problem.Model.Shapes.get_LabeledAs("0", "0", "ПАЗ_" + (paz + i)).Item(1);
                    pazBlock.Select();
                    pazBlock.Label = "ФАЗА_" + phase + "+";
                }

                paz += 2 * q;
                phase++;
                if (paz > halfPazCount) paz = paz - halfPazCount;
                if (phase > phaseCount) phase = 1;

            } while (paz != pazStart);


            pazStart = 1 + phaseCount * q;
            paz = pazStart;


            do
            {
                for (int i = 0; i < q; i++)
                {
                    Shape pazBlock = problem.Model.Shapes.get_LabeledAs("0", "0", "ПАЗ_" + (paz + i)).Item(1);
                    pazBlock.Select();
                    pazBlock.Label = "ФАЗА_" + phase + "-";
                }

                paz += 2 * q;
                phase++;
                if (paz > halfPazCount) paz = paz - halfPazCount;
                if (phase > phaseCount) phase = 1;

            } while (paz != pazStart);

        }


        private void BindBH(LabelBlockMS content, Material mat)
        {
            if (mat.nonLinearBH)
            {
                Spline bh = content.CreateBHCurve();

                foreach (BHPair item in mat.BHSpline)
                {
                    bh.Add(application.PointXY(item.B, item.H));
                }

                content.Spline = bh;
            }
            else
            {
                content.Kxx = mat.muX;
                content.Kyy = mat.muY;
            }

        }

        private void BindMagnete()
        {
            //устанавливаем материал магнитов
            int magnCount = 2 * (int)engine.GetParam("p").Value;

            for (int i = 0; i < magnCount; i++)
            {
                //угол магнитной силы
                double angle = engine.magnetMaterial.powerAngle + Math.PI * i;

                Label labelMagn = problem.Labels[QfShapes.qfBlock].Item("МАГНИТ_" + (i + 1));
                LabelBlockMS content = (LabelBlockMS)labelMagn.Content;

                BindBH(content, engine.magnetMaterial);

                content.Polar = true;
                content.Coercive = application.PointRA(engine.magnetMaterial.magneticPower, angle);
                labelMagn.Content = content;
            }
        }



        public Application GetApplication
        {
            get
            {
                return application;
            }
        }

        void Save()
        {
            problem.SaveAs(workDirectory + "PROBLEM.pbm");
            model.SaveAs(workDirectory + "MODEL.mod");
        }

        private void InfoPoint(String text, Point point)
        {
            OnEventProgress(text + ": x=" + point.X + ", y=" + point.Y, false);
        }

    }
}
