using Google.OrTools.LinearSolver;
using PGOptimisation.Models;

namespace PGOptimisation.Services
{
    public class PGoptimisationService
    {
        public Solver.ResultStatus Optimize(
            PGOptimisationData data,
            out Variable[] gasVars,
            out double[] cokeResults,
            out double[] productionResults,
            out double[] siResults,
            out double totalProduction
        )
        {
            if (data.BaseGas == null || data.BaseGas.Length == 0)
                throw new ArgumentException("Invalid data");

            var solver = Solver.CreateSolver("GLOP");
            int n = data.BaseGas.Length;

            // 1. Инициализация переменных только для газа
            gasVars = new Variable[n];
            for (int i = 0; i < n; i++)
            {
                gasVars[i] = solver.MakeNumVar(data.MinGas[i], data.MaxGas[i], $"gas_{i}");
            }

            // 2. Настройка целевой функции
            var objective = solver.Objective();
            for (int i = 0; i < n; i++)
            {
                double coeff = data.DeltaPgCoeff[i] - data.E[i] * data.DeltaCokeCoeff[i];
                objective.SetCoefficient(gasVars[i], coeff);
            }
            objective.SetMaximization();

            // 3. Ограничения
            // Общий расход газа
            var gasTotal = solver.MakeConstraint(0, data.TotalGasLimit);
            foreach (var var in gasVars) gasTotal.SetCoefficient(var, 1);

            // Ограничения по коксу
            var cokeExpr = new LinearExpr();
            for (int i = 0; i < n; i++)
            {
                cokeExpr += (data.BaseGas[i] - gasVars[i]) * data.E[i];
            }
            solver.Add(cokeExpr <= (data.TotalCokeLimit * 1000 - data.BaseCoke.Sum() * 1000));

            // Ограничения по производству
            var productionExpr = new LinearExpr();
            for (int i = 0; i < n; i++)
            {
                productionExpr += (gasVars[i] - data.BaseGas[i]) * data.DeltaPgCoeff[i]
                                - data.E[i] * (gasVars[i] - data.BaseGas[i]) * data.DeltaCokeCoeff[i];
            }
            solver.Add(productionExpr >= data.RequiredProduction - data.BaseProduction.Sum());

            // Ограничения по кремнию
            for (int i = 0; i < n; i++)
            {
                var siExpr = data.SiBase[i]
                    + (gasVars[i] - data.BaseGas[i]) * data.SiCoeffPg[i]
                    - data.E[i] * (gasVars[i] - data.BaseGas[i]) * data.SiCoeffCoke[i]
                    + ((gasVars[i] - data.BaseGas[i]) * data.DeltaPgCoeff[i]
                       - data.E[i] * (gasVars[i] - data.BaseGas[i]) * data.DeltaCokeCoeff[i])
                       * data.SiCoeffProd[i];

                solver.Add(siExpr <= data.SiMax[i]);
                solver.Add(siExpr >= data.SiMin[i]);
            }

            // 4. Решение задачи
            var result = solver.Solve();

            // 5. Расчет результатов
            cokeResults = new double[n];
            productionResults = new double[n];
            siResults = new double[n];
            totalProduction = 0;

            if (result == Solver.ResultStatus.OPTIMAL)
            {
                for (int i = 0; i < n; i++)
                {
                    double gas = gasVars[i].SolutionValue();

                    // Расчет кокса
                    cokeResults[i] = data.BaseCoke[i] +
                        0.001 * (data.BaseGas[i] - gas) * data.E[i];

                    // Расчет производства
                    productionResults[i] = data.BaseProduction[i]
                        + (gas - data.BaseGas[i]) * data.DeltaPgCoeff[i]
                        - data.E[i] * (gas - data.BaseGas[i]) * data.DeltaCokeCoeff[i];

                    // Расчет Si
                    siResults[i] = data.SiBase[i]
                        + (gas - data.BaseGas[i]) * data.SiCoeffPg[i]
                        - data.E[i] * (gas - data.BaseGas[i]) * data.SiCoeffCoke[i]
                        + ((gas - data.BaseGas[i]) * data.DeltaPgCoeff[i]
                           - data.E[i] * (gas - data.BaseGas[i]) * data.DeltaCokeCoeff[i])
                           * data.SiCoeffProd[i];

                    totalProduction += productionResults[i];
                }
            }

            return result;
        }
    }
}