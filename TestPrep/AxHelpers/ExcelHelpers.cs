using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using GemBox.Spreadsheet;
using TestPrep.Models;

namespace TestPrep.AxHelpers
{
    public class ExcelHelpers
    {
        //public string GenerateNssUploadTemplate()
        //{
        //    using (var db = new AppDbContext())
        //    {
        //        var districts = db.Districts.Where(x => x.Id > 0).ToList();
        //        var filename = DateTime.Now.ToShortDateString() + "NssDataUploadTemplate.xlsx";
        //        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data/Downloads/" + filename);
        //        var ef = new ExcelFile();
        //        var sheet1 = ef.Worksheets.Add("Main");
        //        sheet1.Cells[0, 0].Value = "SURNAME";
        //        sheet1.Cells[0, 1].Value = "FIRSTNAME";
        //        sheet1.Cells[0, 2].Value = "OTHER_NAMES";
        //        sheet1.Cells[0, 3].Value = "DATE_OF_BIRTH";
        //        sheet1.Cells[0, 4].Value = "NSS_NUMBER";
        //        sheet1.Cells[0, 5].Value = "EZWICH_NUMBER";
        //        sheet1.Cells[0, 6].Value = "YEAR";
        //        sheet1.Cells[0, 7].Value = "DISTRICT_NAME";

        //        //Make some notes and some values at the side of the list
        //        var sheet2 = ef.Worksheets.Add("Forbidden");
        //        sheet2.Cells[0, 0].Value = "Please do not Delete or Update any of the values below";
        //        var alphabets = Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (char)i).ToArray();

        //        const int districtsRowStart = 1;
        //        var districtsRowEnd = 0;
        //        for (districtsRowEnd = 0; districtsRowEnd < districts.Count(); districtsRowEnd++) sheet2.Cells[districtsRowStart, districtsRowEnd].Value = districts[districtsRowEnd].Name;
        //        const int rs = districtsRowStart + 1;
        //        var formula1 = "=Forbidden!$A$" + rs + ":$" + alphabets[districtsRowEnd - 1] + "$" + rs;
        //        //set titles
        //        sheet1.DataValidations.Add(new DataValidation(sheet1, "H2:H10000")
        //        {
        //            Type = DataValidationType.List,
        //            Formula1 = formula1,
        //            InputMessageTitle = "Select a district",
        //            InputMessage = "District should be from the list",
        //            ErrorStyle = DataValidationErrorStyle.Warning,
        //            ErrorTitle = "Invalid district",
        //            ErrorMessage = "Value should be from the list"
        //        });

                

        //        sheet1.DataValidations.Add(new DataValidation(sheet1, "D2:D10000")
        //        {
        //            Type = DataValidationType.Date,
        //            Operator = DataValidationOperator.LessThan,
        //            Formula1 = DateTime.Now,
        //            InputMessageTitle = "Enter a date",
        //            InputMessage = "Date should be in the past. Date Format: MM/dd/yyyy ",
        //            ErrorStyle = DataValidationErrorStyle.Stop,
        //            ErrorTitle = "Invalid date",
        //            ErrorMessage = "Value should be in the past"
        //        });
        //        sheet1.Cells[0, 4].Style.NumberFormat = "MM/dd/yyyy";
                


        //        //ef.SaveXlsx(filePath);
        //        ef.Save(filePath);
        //        var bytes = File.ReadAllBytes(filePath);
        //        var file = Convert.ToBase64String(bytes);
        //        return file;
        //    }
        //}
        //public string GenerateManualDisbursements(List<DisbursementDownload> data)
        //{
        //    var filename = DateTime.Now.ToShortDateString() + "ManualDisbursementsDownload.xlsx";
        //    var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data/Downloads/" + filename);

        //    var sheetName = ("Disbursements").Replace(" ", "");
        //    var ef = new ExcelFile();
        //    var ws = ef.Worksheets.Add(sheetName);
        //    var dt = new DataTable();
        //    dt.Columns.Add("REFERENCE", typeof (string));
        //    dt.Columns.Add("USER", typeof (string));
        //    dt.Columns.Add("AMOUNT", typeof (double));
        //    dt.Columns.Add("ACCOUNT NUMBER", typeof (string));
        //    dt.Columns.Add("ACCOUNT NAME", typeof (string));
        //    dt.Columns.Add("PROVIDER", typeof (string));
        //    dt.Columns.Add("TYPE", typeof (string));
        //    foreach (var d in data)
        //    {
        //        dt.Rows.Add(d.Reference, d.User, d.Amount.ToString("##,###.00"),
        //            d.AccountNumber, d.AccountName, d.WalletProvider, d.WalletProviderType);
        //    }
        //    ws.Cells.GetSubrangeAbsolute(0, 0, 0, 7).Merged = true;
        //    ws.Cells[0, 0].Style.HorizontalAlignment = HorizontalAlignmentStyle.Center;
        //    ws.Cells[0, 0].Style.Font.Weight = ExcelFont.BoldWeight;
        //    ws.Cells.GetSubrangeAbsolute(1, 0, 1, 7).Merged = true;
        //    ws.Cells[1, 0].Style.HorizontalAlignment = HorizontalAlignmentStyle.Center;
        //    ws.Cells[1, 0].Style.Font.Weight = ExcelFont.BoldWeight;
        //    ws.Cells[0, 0].Value = "CASHPHASE MICROFINANCE LTD";
        //    ws.Cells[1, 0].Value = "PENDING DISBURSEMENTS";

        //    ws.InsertDataTable(dt,
        //        new InsertDataTableOptions
        //        {
        //            ColumnHeaders = true,
        //            StartRow = 3
        //        });

        //    //ef.SaveXlsx(filePath);
        //    ef.Save(filePath);
        //    var bytes = File.ReadAllBytes(filePath);
        //    var file = Convert.ToBase64String(bytes);
        //    return file;
        //}
    }
}