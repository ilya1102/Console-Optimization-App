using Google.OrTools.LinearSolver;
using PGOptimisation.Models;
using PGOptimisation.Services;

var data = new PGOptimisationData
{
    BaseGas = [15000, 17000, 11000, 13000, 12000],
    MinGas = [10000, 10000, 10000, 10000, 10000],
    MaxGas = [20000, 20000, 20000, 20000, 20000],
    E = [0.59, 0.53, 0.85, 0.59, 0.75],
    BaseCoke = [64.25, 66.76, 56.08, 49.78, 62.92],
    BaseProduction = [146.4, 136.4, 134.3, 122.3, 138.2],
    DeltaPgCoeff = [-0.0007295, -0.0006695, 0, -0.00072373, -0.0007724],
    DeltaCokeCoeff = [-0.00297, -0.00297, -0.002928, -0.002897, -0.00297],
    SiCoeffPg = [0.0001186, 6.34e-05, 6.42e-05, 7.02e-05, 8.14e-05],
    SiCoeffCoke = [0.0001198, 0.0001187, 9.87e-05, 0.000119, 0.000109],
    SiCoeffProd = [-0.0282, -0.0292, -0.03, -0.0229, -0.0277],
    SiBase = [0.59, 0.51, 0.66, 0.6, 0.535],
    SiMin = [0.4, 0.4, 0.4, 0.4, 0.4],
    SiMax = [0.8, 0.8, 0.8, 0.8, 0.8],
    TotalGasLimit = 85000,
    TotalCokeLimit = 300,
    RequiredProduction = 650
};

try
{
    var service = new PGoptimisationService();
    var result = service.Optimize(
        data,
        out var gasVars,
        out var cokeResults,
        out var productionResults,
        out var siResults,
        out var totalProduction
    );

    Console.WriteLine($"Статус решения: {result}");

    if (result == Solver.ResultStatus.OPTIMAL || result == Solver.ResultStatus.FEASIBLE)
    {
        Console.WriteLine("\nОптимальное решение найдено!\n");

        Console.WriteLine("| Печь | Расход ПГ (м³/ч) | Расход кокса (т/ч) | Производительность (т/ч) | Si (%) |");
        Console.WriteLine("|------|------------------|--------------------|--------------------------|--------|");

        for (int i = 0; i < gasVars.Length; i++)
        {
            Console.WriteLine($"| {i + 1,4} | {gasVars[i].SolutionValue(),16:F2} | {cokeResults[i],18:F2} | {productionResults[i],24:F2} | {siResults[i],6:F4} |");
        }

        Console.WriteLine($"\nОбщий расход ПГ: {gasVars.Sum(v => v.SolutionValue()):F2} м³/ч");
        Console.WriteLine($"Общий расход кокса: {cokeResults.Sum():F2} т/ч");
        Console.WriteLine($"Общая производительность: {totalProduction:F2} т/ч");
        Console.WriteLine($"Среднее содержание Si: {siResults.Average():F4}%");

        // Расчет целевой функции Z
        double Z = 0;
        for (int i = 0; i < gasVars.Length; i++)
        {
            Z += (data.DeltaPgCoeff[i] - data.E[i] * data.DeltaCokeCoeff[i]) * gasVars[i].SolutionValue();
        }
        Console.WriteLine($"\nЗначение целевой функции Z: {Z:F4}");
    }
    else
    {
        Console.WriteLine("\nОптимальное решение не найдено. Возможные причины:");
        Console.WriteLine("- Несовместные ограничения");
        Console.WriteLine("- Недостаточно ресурсов для достижения требуемой производительности");
        Console.WriteLine("- Ошибка в исходных данных");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\nОшибка при решении задачи: {ex.Message}");
}