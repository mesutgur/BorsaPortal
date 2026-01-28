using BorsaPortal.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BorsaPortal.Infrastructure.Services
{
    public class ValuationCalculatorService : IValuationCalculatorService
    {
        public ValuationResult CalculateBasicValuation(BasicValuationInput input)
        {
            try
            {
                // Input validation
                if (input.PaidInCapital <= 0)
                {
                    return new ValuationResult
                    {
                        Success = false,
                        Message = "Ödenmiş sermaye değeri sıfır veya negatif olamaz."
                    };
                }

                var result = new ValuationResult { Success = true };

                // HBK = Yıllık Net Kar / Ödenmiş Sermaye
                decimal currentHBK = input.YearlyNetProfit / input.PaidInCapital;
                decimal estimatedHBK = input.EstimatedYearlyNetProfit / input.PaidInCapital;

                // Hisse Fiyat = HBK × F/K Oranı
                decimal currentTargetPrice = currentHBK * input.CurrentPE;
                decimal estimatedTargetPrice = estimatedHBK * input.EstimatedPE;

                result.Calculations.Add(new ValuationCalculationResult
                {
                    CalculationType = "CurrentValuation",
                    CalculationName = "Güncel Değerleme (HBK × F/K)",
                    CalculatedPrice = currentTargetPrice,
                    Formula = "HBK × F/K",
                    FormulaWithValues = $"({input.YearlyNetProfit} / {input.PaidInCapital}) × {input.CurrentPE} = {currentTargetPrice:F2}"
                });

                result.Calculations.Add(new ValuationCalculationResult
                {
                    CalculationType = "EstimatedValuation",
                    CalculationName = "Tahmini Değerleme (Tahmini HBK × Tahmini F/K)",
                    CalculatedPrice = estimatedTargetPrice,
                    Formula = "Tahmini HBK × Tahmini F/K",
                    FormulaWithValues = $"({input.EstimatedYearlyNetProfit} / {input.PaidInCapital}) × {input.EstimatedPE} = {estimatedTargetPrice:F2}"
                });

                result.TargetPrice = estimatedTargetPrice;
                result.AverageTargetPrice = (currentTargetPrice + estimatedTargetPrice) / 2;

                return result;
            }
            catch (Exception ex)
            {
                return new ValuationResult
                {
                    Success = false,
                    Message = $"Hesaplama hatası: {ex.Message}"
                };
            }
        }

        public ValuationResult CalculateQuarter1Valuation(QuarterValuationInput input)
        {
            return CalculateQuarterValuation(input, 4); // 3 aylık × 4 = yıllık
        }

        public ValuationResult CalculateQuarter2Valuation(QuarterValuationInput input)
        {
            return CalculateQuarterValuation(input, 2); // 6 aylık × 2 = yıllık
        }

        public ValuationResult CalculateQuarter3Valuation(QuarterValuationInput input)
        {
            // 9 aylık için: 9 aylık + (9 aylık / 3) = yıllık tahmin
            try
            {
                // Input validation
                if (input.PaidInCapital <= 0 || input.StockPE <= 0 || input.StockPB <= 0 || input.CurrentPrice <= 0)
                {
                    return new ValuationResult
                    {
                        Success = false,
                        Message = "Ödenmiş sermaye, F/K, PD/DD ve güncel fiyat değerleri sıfırdan büyük olmalıdır."
                    };
                }

                var result = new ValuationResult { Success = true };

                // Yıl sonu tahmini net kar = 9 aylık + (9 aylık / 3)
                decimal estimatedQuarterProfit = input.QuarterNetProfit / 3;
                decimal estimatedYearEndProfit = input.QuarterNetProfit + estimatedQuarterProfit;
                result.EstimatedYearEndProfit = estimatedYearEndProfit;

                // Özsermaye hesaplama
                decimal equity = input.Equity ?? (input.CurrentMarketValue ?? (input.CurrentPrice * input.PaidInCapital)) / input.StockPB;

                if (equity == 0)
                {
                    return new ValuationResult
                    {
                        Success = false,
                        Message = "Özsermaye değeri sıfır olamaz. Lütfen özsermaye değerini kontrol edin."
                    };
                }

                // 1) Cari F/K Oranına Göre
                decimal method1 = (input.CurrentPrice / input.StockPE) * input.SectorOrBist100PE;
                result.Calculations.Add(CreateCalculation("CurrentPE", "Cari F/K Oranına Göre Olması Gereken Fiyat",
                    method1, "(Güncel Fiyat / Hisse F/K) × Sektör F/K",
                    $"({input.CurrentPrice:F2} / {input.StockPE:F2}) × {input.SectorOrBist100PE:F2} = {method1:F2}"));

                // Yıl sonu tahmini HBK ve FK
                decimal estimatedHBK = estimatedYearEndProfit / input.PaidInCapital;

                if (estimatedHBK == 0)
                {
                    return new ValuationResult
                    {
                        Success = false,
                        Message = "Tahmini HBK değeri sıfır olamaz. Çeyrek kar değeri çok düşük."
                    };
                }

                decimal estimatedPE = input.CurrentPrice / estimatedHBK;

                // 2) Future's F/K Oranına Göre
                decimal method2 = (input.CurrentPrice / estimatedPE) * input.SectorOrBist100PE;
                result.Calculations.Add(CreateCalculation("FuturePE", "Future's F/K Oranına Göre Olması Gereken Fiyat",
                    method2, "(Güncel Fiyat / Tahmini F/K) × Sektör F/K",
                    $"({input.CurrentPrice:F2} / {estimatedPE:F2}) × {input.SectorOrBist100PE:F2} = {method2:F2}"));

                // 3) PD/DD Oranına Göre
                decimal method3 = (input.CurrentPrice / input.StockPB) * input.SectorOrBist100PB;
                result.Calculations.Add(CreateCalculation("PB", "PD/DD Oranına Göre Olması Gereken Fiyat",
                    method3, "(Güncel Fiyat / Hisse PD/DD) × Sektör PD/DD",
                    $"({input.CurrentPrice:F2} / {input.StockPB:F2}) × {input.SectorOrBist100PB:F2} = {method3:F2}"));

                // 4) Ödenmiş Sermayeye Göre
                decimal method4 = (estimatedYearEndProfit / input.PaidInCapital) * 10;
                result.Calculations.Add(CreateCalculation("PaidCapital", "Ödenmiş Sermayeye Göre Olması Gereken Fiyat",
                    method4, "(Tahmini Yıllık Kar / Ödenmiş Sermaye) × 10",
                    $"({estimatedYearEndProfit:F2} / {input.PaidInCapital:F2}) × 10 = {method4:F2}"));

                // 5) Potansiyel Piyasa Değerine Göre
                decimal potentialMarketValue = (estimatedYearEndProfit * 7) + (equity * 0.5m);
                decimal method5 = potentialMarketValue / input.PaidInCapital;
                result.Calculations.Add(CreateCalculation("PotentialMarketValue", "Potansiyel Piyasa Değerine Göre Olması Gereken Fiyat",
                    method5, "((Tahmini Kar × 7) + (Özsermaye × 0.5)) / Ödenmiş Sermaye",
                    $"(({estimatedYearEndProfit:F2} × 7) + ({equity:F2} × 0.5)) / {input.PaidInCapital:F2} = {method5:F2}"));

                // 6) Özsermaye Karlılığına Göre
                decimal method6 = (estimatedYearEndProfit / equity) * 10 / input.StockPB * input.CurrentPrice;
                result.Calculations.Add(CreateCalculation("ROE", "Özsermaye Karlılığına Göre Olması Gereken Fiyat",
                    method6, "(Tahmini Kar / Özsermaye) × 10 / Hisse PD/DD × Güncel Fiyat",
                    $"({estimatedYearEndProfit:F2} / {equity:F2}) × 10 / {input.StockPB:F2} × {input.CurrentPrice:F2} = {method6:F2}"));

                // Ortalama hedef fiyat
                result.AverageTargetPrice = (method1 + method2 + method3 + method4 + method5 + method6) / 6;
                result.TargetPrice = result.AverageTargetPrice;

                // Prim potansiyeli
                result.PremiumPotential = ((result.AverageTargetPrice.Value - input.CurrentPrice) / input.CurrentPrice) * 100;

                return result;
            }
            catch (Exception ex)
            {
                return new ValuationResult { Success = false, Message = $"Hesaplama hatası: {ex.Message}" };
            }
        }

        public ValuationResult CalculateQuarter4Valuation(QuarterValuationInput input)
        {
            // 4. çeyrek = tam yıllık veri
            try
            {
                // Input validation
                if (input.PaidInCapital <= 0 || input.StockPE <= 0 || input.StockPB <= 0 || input.CurrentPrice <= 0)
                {
                    return new ValuationResult
                    {
                        Success = false,
                        Message = "Ödenmiş sermaye, F/K, PD/DD ve güncel fiyat değerleri sıfırdan büyük olmalıdır."
                    };
                }

                var result = new ValuationResult { Success = true };
                result.EstimatedYearEndProfit = input.QuarterNetProfit; // Zaten yıllık

                decimal equity = input.Equity ?? (input.CurrentMarketValue ?? (input.CurrentPrice * input.PaidInCapital)) / input.StockPB;

                // 1) Cari F/K Oranına Göre
                decimal method1 = (input.CurrentPrice / input.StockPE) * input.SectorOrBist100PE;
                result.Calculations.Add(CreateCalculation("CurrentPE", "Cari F/K Oranına Göre Olması Gereken Fiyat",
                    method1, "(Güncel Fiyat / Hisse F/K) × Sektör F/K",
                    $"({input.CurrentPrice:F2} / {input.StockPE:F2}) × {input.SectorOrBist100PE:F2} = {method1:F2}"));

                // 2) PD/DD Oranına Göre
                decimal method2 = (input.CurrentPrice / input.StockPB) * input.SectorOrBist100PB;
                result.Calculations.Add(CreateCalculation("PB", "PD/DD Oranına Göre Olması Gereken Fiyat",
                    method2, "(Güncel Fiyat / Hisse PD/DD) × Sektör PD/DD",
                    $"({input.CurrentPrice:F2} / {input.StockPB:F2}) × {input.SectorOrBist100PB:F2} = {method2:F2}"));

                // 3) Ödenmiş Sermayeye Göre
                decimal method3 = (input.QuarterNetProfit / input.PaidInCapital) * 10;
                result.Calculations.Add(CreateCalculation("PaidCapital", "Ödenmiş Sermayeye Göre Olması Gereken Fiyat",
                    method3, "(Yıllık Kar / Ödenmiş Sermaye) × 10",
                    $"({input.QuarterNetProfit:F2} / {input.PaidInCapital:F2}) × 10 = {method3:F2}"));

                // 4) Potansiyel Piyasa Değerine Göre
                decimal potentialMarketValue = (input.QuarterNetProfit * 7) + (equity * 0.5m);
                decimal method4 = potentialMarketValue / input.PaidInCapital;
                result.Calculations.Add(CreateCalculation("PotentialMarketValue", "Potansiyel Piyasa Değerine Göre Olması Gereken Fiyat",
                    method4, "((Yıllık Kar × 7) + (Özsermaye × 0.5)) / Ödenmiş Sermaye",
                    $"(({input.QuarterNetProfit:F2} × 7) + ({equity:F2} × 0.5)) / {input.PaidInCapital:F2} = {method4:F2}"));

                // 5) Özsermaye Karlılığına Göre
                decimal method5 = (input.QuarterNetProfit / equity) * 10 / input.StockPB * input.CurrentPrice;
                result.Calculations.Add(CreateCalculation("ROE", "Özsermaye Karlılığına Göre Olması Gereken Fiyat",
                    method5, "(Yıllık Kar / Özsermaye) × 10 / Hisse PD/DD × Güncel Fiyat",
                    $"({input.QuarterNetProfit:F2} / {equity:F2}) × 10 / {input.StockPB:F2} × {input.CurrentPrice:F2} = {method5:F2}"));

                // Ortalama hedef fiyat (5 yöntem)
                result.AverageTargetPrice = (method1 + method2 + method3 + method4 + method5) / 5;
                result.TargetPrice = result.AverageTargetPrice;

                // Prim potansiyeli
                result.PremiumPotential = ((result.AverageTargetPrice.Value - input.CurrentPrice) / input.CurrentPrice) * 100;

                return result;
            }
            catch (Exception ex)
            {
                return new ValuationResult { Success = false, Message = $"Hesaplama hatası: {ex.Message}" };
            }
        }

        public ValuationResult CalculateYearEndEstimateValuation(YearEndEstimateInput input)
        {
            try
            {
                // Input validation
                if (input.PaidInCapital <= 0 || input.CurrentPrice <= 0)
                {
                    return new ValuationResult
                    {
                        Success = false,
                        Message = "Ödenmiş sermaye ve güncel fiyat değerleri sıfırdan büyük olmalıdır."
                    };
                }

                var result = new ValuationResult { Success = true };
                result.EstimatedYearEndProfit = input.EstimatedYearlyNetProfit;

                // Tahmini HBK
                decimal estimatedHBK = input.EstimatedYearlyNetProfit / input.PaidInCapital;

                if (estimatedHBK == 0)
                {
                    return new ValuationResult
                    {
                        Success = false,
                        Message = "Tahmini HBK değeri sıfır olamaz. Lütfen tahmini yıllık net kar değerini kontrol edin."
                    };
                }

                // Future's F/K (Tahmini)
                decimal estimatedPE = input.CurrentPrice / estimatedHBK;

                // Hedef Fiyat = (Güncel Fiyat / Tahmini F/K) × Sektör F/K
                decimal targetPrice = (input.CurrentPrice / estimatedPE) * input.SectorOrBist100PE;

                result.Calculations.Add(CreateCalculation("YearEndEstimate", "Yıl Sonu Tahminine Göre Hedef Fiyat",
                    targetPrice, "(Güncel Fiyat / Tahmini F/K) × Sektör F/K",
                    $"({input.CurrentPrice:F2} / {estimatedPE:F2}) × {input.SectorOrBist100PE:F2} = {targetPrice:F2}"));

                result.TargetPrice = targetPrice;
                result.AverageTargetPrice = targetPrice;

                // Prim potansiyeli
                result.PremiumPotential = ((targetPrice - input.CurrentPrice) / input.CurrentPrice) * 100;

                return result;
            }
            catch (Exception ex)
            {
                return new ValuationResult { Success = false, Message = $"Hesaplama hatası: {ex.Message}" };
            }
        }

        public ValuationResult CalculateLongTermTargetValuation(LongTermTargetInput input)
        {
            try
            {
                // Input validation
                if (input.PaidInCapital <= 0 || input.CurrentPrice <= 0)
                {
                    return new ValuationResult
                    {
                        Success = false,
                        Message = "Ödenmiş sermaye ve güncel fiyat değerleri sıfırdan büyük olmalıdır."
                    };
                }

                if (input.NetProfitYear1 == 0 || input.NetProfitYear2 == 0 || input.NetProfitYear3 == 0)
                {
                    return new ValuationResult
                    {
                        Success = false,
                        Message = "Geçmiş yıl net kar değerleri sıfır olamaz."
                    };
                }

                if (input.EquityYear1 == 0 || input.EquityYear2 == 0 || input.EquityYear3 == 0)
                {
                    return new ValuationResult
                    {
                        Success = false,
                        Message = "Geçmiş yıl özsermaye değerleri sıfır olamaz."
                    };
                }

                var result = new ValuationResult { Success = true };

                // Kar artış oranları (Year1=En eski, Year4=En yeni)
                decimal profitGrowthYear2 = (input.NetProfitYear2 - input.NetProfitYear1) / Math.Abs(input.NetProfitYear1);
                decimal profitGrowthYear3 = (input.NetProfitYear3 - input.NetProfitYear2) / Math.Abs(input.NetProfitYear2);
                decimal profitGrowthYear4 = (input.NetProfitYear4 - input.NetProfitYear3) / Math.Abs(input.NetProfitYear3);
                decimal avgProfitGrowth = (profitGrowthYear2 + profitGrowthYear3 + profitGrowthYear4) / 3;

                // Özsermaye artış oranları
                decimal equityGrowthYear2 = (input.EquityYear2 - input.EquityYear1) / input.EquityYear1;
                decimal equityGrowthYear3 = (input.EquityYear3 - input.EquityYear2) / input.EquityYear2;
                decimal equityGrowthYear4 = (input.EquityYear4 - input.EquityYear3) / input.EquityYear3;
                decimal avgEquityGrowth = (equityGrowthYear2 + equityGrowthYear3 + equityGrowthYear4) / 3;

                // Gelecek yıl tahmini net kar
                decimal estimatedNextYearProfit = (input.NetProfitYear4 * avgProfitGrowth) + input.NetProfitYear4;
                result.EstimatedYearEndProfit = estimatedNextYearProfit;

                // Gelecek yıl tahmini özsermaye
                decimal estimatedNextYearEquity = (input.EquityYear4 * avgEquityGrowth) + input.EquityYear4;

                // Yıl sonu tahmini HBK ve F/K
                decimal estimatedHBK = estimatedNextYearProfit / input.PaidInCapital;

                if (estimatedHBK == 0)
                {
                    return new ValuationResult
                    {
                        Success = false,
                        Message = "Tahmini HBK değeri sıfır olamaz. Kar tahmini çok düşük."
                    };
                }

                decimal estimatedPE = input.CurrentPrice / estimatedHBK;

                // 1) Yıl Sonu Tahmini F/K'ya Göre
                decimal method1 = (input.CurrentPrice / estimatedPE) * input.SectorOrBist100PE;
                result.Calculations.Add(CreateCalculation("YearEndPE", "Yıl Sonu Tahmini F/K'ya Göre",
                    method1, "(Güncel Fiyat / Tahmini F/K) × Sektör F/K",
                    $"({input.CurrentPrice:F2} / {estimatedPE:F2}) × {input.SectorOrBist100PE:F2} = {method1:F2}"));

                // 2) Ödenmiş Sermayeye Göre
                decimal method2 = (estimatedNextYearProfit / input.PaidInCapital) * 10;
                result.Calculations.Add(CreateCalculation("PaidCapital", "Ödenmiş Sermayeye Göre",
                    method2, "(Tahmini Kar / Ödenmiş Sermaye) × 10",
                    $"({estimatedNextYearProfit:F2} / {input.PaidInCapital:F2}) × 10 = {method2:F2}"));

                // 3) Potansiyel Piyasa Değerine Göre
                decimal potentialMarketValue = (estimatedNextYearProfit * 7) + (0.5m * estimatedNextYearEquity);
                decimal method3 = potentialMarketValue / input.PaidInCapital;
                result.Calculations.Add(CreateCalculation("PotentialMarketValue", "Potansiyel Piyasa Değerine Göre",
                    method3, "((Tahmini Kar × 7) + (Tahmini Özsermaye × 0.5)) / Ödenmiş Sermaye",
                    $"(({estimatedNextYearProfit:F2} × 7) + ({estimatedNextYearEquity:F2} × 0.5)) / {input.PaidInCapital:F2} = {method3:F2}"));

                // Ortalama hedef fiyat
                result.AverageTargetPrice = (method1 + method2 + method3) / 3;
                result.TargetPrice = result.AverageTargetPrice;

                // Prim potansiyeli
                result.PremiumPotential = ((result.AverageTargetPrice.Value - input.CurrentPrice) / input.CurrentPrice) * 100;

                return result;
            }
            catch (Exception ex)
            {
                return new ValuationResult { Success = false, Message = $"Hesaplama hatası: {ex.Message}" };
            }
        }

        private ValuationResult CalculateQuarterValuation(QuarterValuationInput input, decimal multiplier)
        {
            try
            {
                // Input validation
                if (input.PaidInCapital <= 0 || input.StockPE <= 0 || input.StockPB <= 0 || input.CurrentPrice <= 0)
                {
                    return new ValuationResult
                    {
                        Success = false,
                        Message = "Ödenmiş sermaye, F/K, PD/DD ve güncel fiyat değerleri sıfırdan büyük olmalıdır."
                    };
                }

                var result = new ValuationResult { Success = true };

                // Yıl sonu tahmini net kar
                decimal estimatedYearEndProfit = input.QuarterNetProfit * multiplier;
                result.EstimatedYearEndProfit = estimatedYearEndProfit;

                // Özsermaye hesaplama
                decimal equity = input.Equity ?? (input.CurrentMarketValue ?? (input.CurrentPrice * input.PaidInCapital)) / input.StockPB;

                if (equity == 0)
                {
                    return new ValuationResult
                    {
                        Success = false,
                        Message = "Özsermaye değeri sıfır olamaz. Lütfen özsermaye değerini kontrol edin."
                    };
                }

                // 1) Cari F/K Oranına Göre
                decimal method1 = (input.CurrentPrice / input.StockPE) * input.SectorOrBist100PE;
                result.Calculations.Add(CreateCalculation("CurrentPE", "Cari F/K Oranına Göre Olması Gereken Fiyat",
                    method1, "(Güncel Fiyat / Hisse F/K) × Sektör F/K",
                    $"({input.CurrentPrice:F2} / {input.StockPE:F2}) × {input.SectorOrBist100PE:F2} = {method1:F2}"));

                // Yıl sonu tahmini HBK ve FK
                decimal estimatedHBK = estimatedYearEndProfit / input.PaidInCapital;

                if (estimatedHBK == 0)
                {
                    return new ValuationResult
                    {
                        Success = false,
                        Message = "Tahmini HBK değeri sıfır olamaz. Çeyrek kar değeri çok düşük."
                    };
                }

                decimal estimatedPE = input.CurrentPrice / estimatedHBK;

                // 2) Future's F/K Oranına Göre
                decimal method2 = (input.CurrentPrice / estimatedPE) * input.SectorOrBist100PE;
                result.Calculations.Add(CreateCalculation("FuturePE", "Future's F/K Oranına Göre Olması Gereken Fiyat",
                    method2, "(Güncel Fiyat / Tahmini F/K) × Sektör F/K",
                    $"({input.CurrentPrice:F2} / {estimatedPE:F2}) × {input.SectorOrBist100PE:F2} = {method2:F2}"));

                // 3) PD/DD Oranına Göre
                decimal method3 = (input.CurrentPrice / input.StockPB) * input.SectorOrBist100PB;
                result.Calculations.Add(CreateCalculation("PB", "PD/DD Oranına Göre Olması Gereken Fiyat",
                    method3, "(Güncel Fiyat / Hisse PD/DD) × Sektör PD/DD",
                    $"({input.CurrentPrice:F2} / {input.StockPB:F2}) × {input.SectorOrBist100PB:F2} = {method3:F2}"));

                // 4) Ödenmiş Sermayeye Göre
                decimal multiplierForMethod4 = input.QuarterNumber == 2 ? 10.02m : 10m;
                decimal method4 = (estimatedYearEndProfit / input.PaidInCapital) * multiplierForMethod4;
                result.Calculations.Add(CreateCalculation("PaidCapital", "Ödenmiş Sermayeye Göre Olması Gereken Fiyat",
                    method4, $"(Tahmini Yıllık Kar / Ödenmiş Sermaye) × {multiplierForMethod4}",
                    $"({estimatedYearEndProfit:F2} / {input.PaidInCapital:F2}) × {multiplierForMethod4} = {method4:F2}"));

                // 5) Potansiyel Piyasa Değerine Göre
                decimal potentialMarketValue = (estimatedYearEndProfit * 7) + (equity * 0.5m);
                decimal method5 = potentialMarketValue / input.PaidInCapital;
                result.Calculations.Add(CreateCalculation("PotentialMarketValue", "Potansiyel Piyasa Değerine Göre Olması Gereken Fiyat",
                    method5, "((Tahmini Kar × 7) + (Özsermaye × 0.5)) / Ödenmiş Sermaye",
                    $"(({estimatedYearEndProfit:F2} × 7) + ({equity:F2} × 0.5)) / {input.PaidInCapital:F2} = {method5:F2}"));

                // 6) Özsermaye Karlılığına Göre
                decimal method6 = (estimatedYearEndProfit / equity) * 10 / input.StockPB * input.CurrentPrice;
                result.Calculations.Add(CreateCalculation("ROE", "Özsermaye Karlılığına Göre Olması Gereken Fiyat",
                    method6, "(Tahmini Kar / Özsermaye) × 10 / Hisse PD/DD × Güncel Fiyat",
                    $"({estimatedYearEndProfit:F2} / {equity:F2}) × 10 / {input.StockPB:F2} × {input.CurrentPrice:F2} = {method6:F2}"));

                // Ortalama hedef fiyat
                result.AverageTargetPrice = (method1 + method2 + method3 + method4 + method5 + method6) / 6;
                result.TargetPrice = result.AverageTargetPrice;

                // Prim potansiyeli
                result.PremiumPotential = ((result.AverageTargetPrice.Value - input.CurrentPrice) / input.CurrentPrice) * 100;

                return result;
            }
            catch (Exception ex)
            {
                return new ValuationResult { Success = false, Message = $"Hesaplama hatası: {ex.Message}" };
            }
        }

        private ValuationCalculationResult CreateCalculation(string type, string name, decimal price, string formula, string formulaWithValues)
        {
            return new ValuationCalculationResult
            {
                CalculationType = type,
                CalculationName = name,
                CalculatedPrice = price,
                Formula = formula,
                FormulaWithValues = formulaWithValues
            };
        }

        public ValuationResult CalculateHistoricalPEValuation(HistoricalPEInput input)
        {
            try
            {
                // Input validation
                if (input.CurrentPrice <= 0 || input.CurrentPE <= 0)
                {
                    return new ValuationResult
                    {
                        Success = false,
                        Message = "Güncel fiyat ve güncel F/K değerleri sıfırdan büyük olmalıdır."
                    };
                }

                var result = new ValuationResult { Success = true };

                // Tarihsel F/K Oranı: Son üç yılın ortalama F/K oranı
                decimal historicalPE = (input.PE_Year1 + input.PE_Year2 + input.PE_Year3) / 3;

                // Tarihsel F/K'ya Göre Hedef Fiyat = (Hisse Fiyatı / Şuan ki FK Oranı) × Tarihsel FK Oranı
                decimal targetPrice = (input.CurrentPrice / input.CurrentPE) * historicalPE;

                result.Calculations.Add(CreateCalculation(
                    "HistoricalPE",
                    "Tarihsel F/K Oranına Göre Hissenin Değeri",
                    targetPrice,
                    "(Hisse Fiyatı / Şuan ki F/K) × Tarihsel F/K",
                    $"({input.CurrentPrice:F2} / {input.CurrentPE:F2}) × {historicalPE:F2} = {targetPrice:F2}"
                ));

                result.TargetPrice = targetPrice;
                result.AverageTargetPrice = targetPrice;

                // Prim potansiyeli
                result.PremiumPotential = ((targetPrice - input.CurrentPrice) / input.CurrentPrice) * 100;

                // Tarihsel F/K bilgisi ekle (dinamik yıl bilgisi ile)
                int year1 = DateTime.Now.Year - 3;
                int year2 = DateTime.Now.Year - 2;
                int year3 = DateTime.Now.Year - 1;

                result.Calculations.Add(CreateCalculation(
                    "Info",
                    $"Tarihsel F/K Oranı ({year1}-{year3} Ortalaması)",
                    historicalPE,
                    $"Tarihsel F/K = ({year1} F/K + {year2} F/K + {year3} F/K) / 3",
                    $"({input.PE_Year1:F2} + {input.PE_Year2:F2} + {input.PE_Year3:F2}) / 3 = {historicalPE:F2}"
                ));

                return result;
            }
            catch (Exception ex)
            {
                return new ValuationResult
                {
                    Success = false,
                    Message = $"Hesaplama hatası: {ex.Message}"
                };
            }
        }

        public ValuationResult CalculatePriceToSalesValuation(PriceToSalesInput input)
        {
            try
            {
                // Input validation
                if (input.YearlySales <= 0 || input.CurrentMarketValue <= 0 || input.CurrentPrice <= 0)
                {
                    return new ValuationResult
                    {
                        Success = false,
                        Message = "Yıllık satış, piyasa değeri ve güncel fiyat değerleri sıfırdan büyük olmalıdır."
                    };
                }

                var result = new ValuationResult { Success = true };

                // Net Kar Marjı = (Yıllık Net Kar / Yıllık Net Satış) × 10
                decimal netProfitMargin = (input.YearlyNetProfit / input.YearlySales) * 10;

                // PD/NS = Piyasa Değeri / Yıllık Net Satış
                decimal priceToSales = input.CurrentMarketValue / input.YearlySales;

                if (priceToSales == 0)
                {
                    return new ValuationResult
                    {
                        Success = false,
                        Message = "PD/NS oranı sıfır olamaz."
                    };
                }

                // Değer Biçme = (Net Kar Marjı / PD/NS)
                decimal valuationMultiplier = netProfitMargin / priceToSales;

                // Hedef Fiyat = Hisse Fiyatı × Değer Biçme Katsayısı
                decimal targetPrice = input.CurrentPrice * valuationMultiplier;

                // Prim potansiyeli
                decimal premiumPotential = valuationMultiplier;
                result.PremiumPotential = (premiumPotential - 1) * 100;

                result.Calculations.Add(CreateCalculation(
                    "NetProfitMargin",
                    "Net Kar Marjı",
                    netProfitMargin,
                    "Net Kar Marjı = (Yıllık Net Kar / Yıllık Net Satış) × 10",
                    $"({input.YearlyNetProfit:F2} / {input.YearlySales:F2}) × 10 = {netProfitMargin:F2}"
                ));

                result.Calculations.Add(CreateCalculation(
                    "PriceToSales",
                    "PD/NS Oranı",
                    priceToSales,
                    "PD/NS = Piyasa Değeri / Yıllık Net Satış",
                    $"{input.CurrentMarketValue:F2} / {input.YearlySales:F2} = {priceToSales:F2}"
                ));

                result.Calculations.Add(CreateCalculation(
                    "ValuationMultiplier",
                    "Değer Biçme Katsayısı (Prim Potansiyeli)",
                    valuationMultiplier,
                    "Değer Biçme = Net Kar Marjı / PD/NS",
                    $"{netProfitMargin:F2} / {priceToSales:F2} = {valuationMultiplier:F2}"
                ));

                result.Calculations.Add(CreateCalculation(
                    "TargetPrice",
                    "PD/NS Yöntemine Göre Olması Gereken Fiyat",
                    targetPrice,
                    "Hedef Fiyat = Hisse Fiyatı × Değer Biçme",
                    $"{input.CurrentPrice:F2} × {valuationMultiplier:F2} = {targetPrice:F2}"
                ));

                result.TargetPrice = targetPrice;
                result.AverageTargetPrice = targetPrice;

                return result;
            }
            catch (Exception ex)
            {
                return new ValuationResult
                {
                    Success = false,
                    Message = $"Hesaplama hatası: {ex.Message}"
                };
            }
        }

        public decimal CalculateBonusSharePotential(decimal equity, decimal paidInCapital)
        {
            // Bedelsiz Potansiyeli = (Öz Sermaye - Ödenmiş Sermaye) / Ödenmiş Sermaye × 100
            if (paidInCapital == 0)
                return 0;

            return ((equity - paidInCapital) / paidInCapital) * 100;
        }
    }
}
