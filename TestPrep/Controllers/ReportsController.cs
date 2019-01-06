using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using PdfSharp;
using TaxPayCoAPI.AxHelpers;
using TaxPayCoAPI.DataAccess.Filters;
using TaxPayCoAPI.Models;

namespace TaxPayCoAPI.Controllers
{
    [RoutePrefix("api/reports")]
    //[Authorize]
    public class ReportsController : ApiController
    {
        [Route("userslist")]
        public ResultObj UsersList(UserFilter filter)
        {
            ResultObj results;
            try
            {
                //var user = User.Identity.AsAppUser().Result;
                var users = filter.BuildQuery(new AppDbContext().Users).OrderBy(x => x.CreatedAt).ToList().Select(x => new
                {
                    x.Id,
                    x.UserType,
                    Name = x.Name.ToUpper(),
                    Gender = x.Gender?.ToString().ToUpper(),
                    Email = x.Email?.ToUpper(),
                    x.DateOfBirth,
                    Nationality = x.Nationality?.Name?.ToUpper(),
                    x.PhoneNumber,
                    Status = x.Status.ToString().ToUpper()
                }).ToList();

                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/UsersList.html"));

                //Write company details
                template = template.Replace("[COMPANYNAME]", SetupConfig.Setting.CompanyDetails.Name?.ToUpper());
                template = template.Replace("[COMPANYADDRESS]", SetupConfig.Setting.CompanyDetails.Address?.ToUpper());
                template = template.Replace("[COMPANYLOGO]", SetupConfig.Setting.CompanyDetails.Logo);

                var list = string.Empty;
                var num = 0;

                foreach (var user in users)
                {
                    var dob = user.DateOfBirth??DateTime.Now;
                    var age = DateHelpers.GetAge(dob);
                    var ag = age > 0 ? age.ToString() : "";
                    num++;
                    list += "<tr style='border:none;font-size: 12px; text-transform: uppercase !important;'>" +
                            $"<td>{num}</td>" +
                            $"<td>{user.UserType}</td>" +
                            $"<td>{user.Name}</td>" +
                            $"<td>{ag}</td>" +
                            $"<td>{user.Gender}</td>" +
                            $"<td>{user.PhoneNumber}</td>" +
                            $"<td>{user.Email}</td>" +
                            $"<td>{user.Nationality}</td>" +
                            $"<td>{user.Status}</td> </tr>";
                }

                template = template.Replace("[LIST]", list);
                template = template.Replace("[TOTAL]", num.ToString());

                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4, true);
                results = WebHelpers.BuildResponse(pdfOutput, "Users List Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [Route("newclients")]
        public ResultObj NewClientsList(PeriodicReportsFilter filter)
        {
            ResultObj results;
            try
            {
                //var user = User.Identity.AsAppUser().Result;
                var db = new AppDbContext();
                //check period
                if(filter.StartDate == null) throw new Exception("Please specify the start date.");
                if (filter.EndDate == null) throw new Exception("Please specify the end date.");
                var sd = new DateTime(filter.StartDate.Value.Year, filter.StartDate.Value.Month, filter.StartDate.Value.Day, 0, 0, 0);
                var ed = new DateTime(filter.EndDate.Value.Year, filter.EndDate.Value.Month, filter.EndDate.Value.Day, 23, 59, 59);
                var clients = db.Users.Where(x=> x.CreatedAt >= sd && x.CreatedAt <= ed && x.UserType != UserType.System).OrderBy(x=> x.CreatedAt).ToList().Select(x => new
                {
                    x.Id,
                    x.UserType,
                    Name = x.Name.ToUpper(),
                    Gender = x.Gender?.ToString().ToUpper(),
                    Email = x.Email?.ToUpper(),
                    x.DateOfBirth,
                    Nationality = x.Nationality?.Name?.ToUpper(),
                    x.PhoneNumber,
                    Status = x.Status.ToString().ToUpper(),
                    CreatedAt = x.CreatedAt.ToShortDateString()
                }).ToList();

                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/NewClientsList.html"));

                //Write company details
                template = template.Replace("[COMPANYNAME]", SetupConfig.Setting.CompanyDetails.Name?.ToUpper());
                template = template.Replace("[COMPANYADDRESS]", SetupConfig.Setting.CompanyDetails.Address?.ToUpper());
                template = template.Replace("[COMPANYLOGO]", SetupConfig.Setting.CompanyDetails.Logo);
                template = template.Replace("[STARTDATE]", sd.ToShortDateString());
                template = template.Replace("[ENDDATE]", ed.ToShortDateString());

                var list = string.Empty;
                var num = 0;

                foreach (var client in clients)
                {
                    var dob = client.DateOfBirth ?? DateTime.Now;
                    var age = DateHelpers.GetAge(dob);
                    var ag = age > 0 ? age.ToString() : "";
                    num++;
                    list += "<tr style='border:none;font-size: 12px; text-transform: uppercase !important;'>" +
                            $"<td>{num}</td>" +
                            $"<td>{client.UserType}</td>" +
                            $"<td>{client.Name}</td>" +
                            $"<td>{ag}</td>" +
                            $"<td>{client.Gender}</td>" +
                            $"<td>{client.PhoneNumber}</td>" +
                            $"<td>{client.Email}</td>" +
                            $"<td>{client.Nationality}</td>" +
                            $"<td>{client.CreatedAt}</td> </tr>";
                }

                template = template.Replace("[LIST]", list);
                template = template.Replace("[TOTAL]", num.ToString());

                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4, true);
                results = WebHelpers.BuildResponse(pdfOutput, "Clients List Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [Route("newfemaleclients")]
        public ResultObj NewFemaleClientsList(PeriodicReportsFilter filter)
        {
            ResultObj results;
            try
            {
                //var user = User.Identity.AsAppUser().Result;
                var db = new AppDbContext();
                //check period
                if (filter.StartDate == null) throw new Exception("Please specify the start date.");
                if (filter.EndDate == null) throw new Exception("Please specify the end date.");
                var sd = new DateTime(filter.StartDate.Value.Year, filter.StartDate.Value.Month, filter.StartDate.Value.Day, 0, 0, 0);
                var ed = new DateTime(filter.EndDate.Value.Year, filter.EndDate.Value.Month, filter.EndDate.Value.Day, 23, 59, 59);
                var clients = db.Users.Where(x => x.Gender == Gender.Female && x.CreatedAt >= sd && x.CreatedAt <= ed && x.UserType != UserType.System).OrderBy(x => x.CreatedAt).ToList().Select(x => new
                {
                    x.Id,
                    x.UserType,
                    Name = x.Name.ToUpper(),
                    Gender = x.Gender?.ToString().ToUpper(),
                    Email = x.Email?.ToUpper(),
                    x.DateOfBirth,
                    Nationality = x.Nationality?.Name?.ToUpper(),
                    x.PhoneNumber,
                    Status = x.Status.ToString().ToUpper(),
                    CreatedAt = x.CreatedAt.ToShortDateString()
                }).ToList();

                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/FemaleClientsList.html"));

                //Write company details
                template = template.Replace("[COMPANYNAME]", SetupConfig.Setting.CompanyDetails.Name?.ToUpper());
                template = template.Replace("[COMPANYADDRESS]", SetupConfig.Setting.CompanyDetails.Address?.ToUpper());
                template = template.Replace("[COMPANYLOGO]", SetupConfig.Setting.CompanyDetails.Logo);
                template = template.Replace("[STARTDATE]", sd.ToShortDateString());
                template = template.Replace("[ENDDATE]", ed.ToShortDateString());

                var list = string.Empty;
                var num = 0;

                foreach (var client in clients)
                {
                    var dob = client.DateOfBirth ?? DateTime.Now;
                    var age = DateHelpers.GetAge(dob);
                    var ag = age > 0 ? age.ToString() : "";
                    num++;
                    list += "<tr style='border:none;font-size: 12px; text-transform: uppercase !important;'>" +
                            $"<td>{num}</td>" +
                            $"<td>{client.UserType}</td>" +
                            $"<td>{client.Name}</td>" +
                            $"<td>{ag}</td>" +
                            $"<td>{client.PhoneNumber}</td>" +
                            $"<td>{client.Email}</td>" +
                            $"<td>{client.Nationality}</td>" +
                            $"<td>{client.CreatedAt}</td> </tr>";
                }

                template = template.Replace("[LIST]", list);
                template = template.Replace("[TOTAL]", num.ToString());

                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4, true);
                results = WebHelpers.BuildResponse(pdfOutput, "Female Clients List Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [Route("activeborrowers")]
        public ResultObj ActiveBorrowers(PeriodicReportsFilter filter)
        {
            ResultObj results;
            try
            {
                //var user = User.Identity.AsAppUser().Result;
                var db = new AppDbContext();
                //check period
                if (filter.StartDate == null) throw new Exception("Please specify the start date.");
                if (filter.EndDate == null) throw new Exception("Please specify the end date.");
                var sd = new DateTime(filter.StartDate.Value.Year, filter.StartDate.Value.Month, filter.StartDate.Value.Day, 0, 0, 0);
                var ed = new DateTime(filter.EndDate.Value.Year, filter.EndDate.Value.Month, filter.EndDate.Value.Day, 23, 59, 59);
                var loans = db.Loans.Where(x=> (x.Status == LoanStatus.Active || x.Status== LoanStatus.Overdue) && x.Date >= sd && x.Date <= ed).Include(x=> x.Disbursement.Request.User).ToList().Select(x => new
                {
                    x.Id,
                    x.AmountPayable,
                    Name = x.Disbursement?.Request?.User?.Name?.ToUpper(),
                    Gender = x.Disbursement?.Request?.User?.Gender?.ToString().ToUpper(),
                    Email = x.Disbursement?.Request?.User?.Email?.ToUpper(),
                    x.Disbursement?.Request?.User?.DateOfBirth,
                    x.Disbursement?.Request?.User?.UserType,
                    Nationality = x.Disbursement?.Request?.User?.Nationality?.Name?.ToUpper(),
                    Status = x.Status.ToString().ToUpper()
                }).ToList();

                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/ActiveBorrowers.html"));

                //Write company details
                template = template.Replace("[COMPANYNAME]", SetupConfig.Setting.CompanyDetails.Name?.ToUpper());
                template = template.Replace("[COMPANYADDRESS]", SetupConfig.Setting.CompanyDetails.Address?.ToUpper());
                template = template.Replace("[COMPANYLOGO]", SetupConfig.Setting.CompanyDetails.Logo);
                template = template.Replace("[STARTDATE]", sd.ToShortDateString());
                template = template.Replace("[ENDDATE]", ed.ToShortDateString());

                var list = string.Empty;
                var num = 0;

                foreach (var item in loans)
                {
                    var dob = item.DateOfBirth ?? DateTime.Now;
                    var age = DateHelpers.GetAge(dob);
                    var ag = age > 0 ? age.ToString() : "";
                    num++;
                    list += "<tr style='border:none;font-size: 12px; text-transform: uppercase !important;'>" +
                            $"<td>{num}</td>" +
                            $"<td>{item.Name}</td>" +
                            $"<td>{ag}</td>" +
                            $"<td>{item.Gender}</td>" +
                            $"<td>{item.Nationality}</td>" +
                            $"<td>{item.UserType}</td>" +
                            $"<td class='text-right'>{item.AmountPayable.ToString(DefaultKeys.AmountFormat)}</td> </tr>";
                }
                var totalAmount = loans.Sum(x => x.AmountPayable);

                template = template.Replace("[LIST]", list);
                template = template.Replace("[TOTAL]", num.ToString());
                template = template.Replace("[TOTALAMOUNT]", totalAmount.ToString(DefaultKeys.AmountFormat));

                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4, true);
                results = WebHelpers.BuildResponse(pdfOutput, "Active Borrowers Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [Route("activefemaleborrowers")]
        public ResultObj ActiveFemaleBorrowers(PeriodicReportsFilter filter)
        {
            ResultObj results;
            try
            {
                //var user = User.Identity.AsAppUser().Result;
                var db = new AppDbContext();
                //check period
                if (filter.StartDate == null) throw new Exception("Please specify the start date.");
                if (filter.EndDate == null) throw new Exception("Please specify the end date.");
                var sd = new DateTime(filter.StartDate.Value.Year, filter.StartDate.Value.Month, filter.StartDate.Value.Day, 0, 0, 0);
                var ed = new DateTime(filter.EndDate.Value.Year, filter.EndDate.Value.Month, filter.EndDate.Value.Day, 23, 59, 59);
                var loans = db.Loans.Where(x => (x.Status == LoanStatus.Active || x.Status == LoanStatus.Overdue) && x.Date >= sd && x.Date <= ed && x.Disbursement.Request.User.Gender == Gender.Female).Include(x => x.Disbursement.Request.User).ToList().Select(x => new
                {
                    x.Id,
                    x.AmountPayable,
                    Name = x.Disbursement?.Request?.User?.Name?.ToUpper(),
                    Gender = x.Disbursement?.Request?.User?.Gender?.ToString().ToUpper(),
                    Email = x.Disbursement?.Request?.User?.Email?.ToUpper(),
                    x.Disbursement?.Request?.User?.DateOfBirth,
                    x.Disbursement?.Request?.User?.UserType,
                    Nationality = x.Disbursement?.Request?.User?.Nationality?.Name?.ToUpper(),
                    Status = x.Status.ToString().ToUpper()
                }).ToList();

                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/ActiveFemaleBorrowers.html"));

                //Write company details
                template = template.Replace("[COMPANYNAME]", SetupConfig.Setting.CompanyDetails.Name?.ToUpper());
                template = template.Replace("[COMPANYADDRESS]", SetupConfig.Setting.CompanyDetails.Address?.ToUpper());
                template = template.Replace("[COMPANYLOGO]", SetupConfig.Setting.CompanyDetails.Logo);
                template = template.Replace("[STARTDATE]", sd.ToShortDateString());
                template = template.Replace("[ENDDATE]", ed.ToShortDateString());

                var list = string.Empty;
                var num = 0;

                foreach (var item in loans)
                {
                    var dob = item.DateOfBirth ?? DateTime.Now;
                    var age = DateHelpers.GetAge(dob);
                    var ag = age > 0 ? age.ToString() : "";
                    num++;
                    list += "<tr style='border:none;font-size: 12px; text-transform: uppercase !important;'>" +
                            $"<td>{num}</td>" +
                            $"<td>{item.Name}</td>" +
                            $"<td>{ag}</td>" +
                            $"<td>{item.Nationality}</td>" +
                            $"<td>{item.UserType}</td>" +
                            $"<td class='text-right'>{item.AmountPayable.ToString(DefaultKeys.AmountFormat)}</td></tr>";
                }
                var totalAmount = loans.Sum(x => x.AmountPayable);

                template = template.Replace("[LIST]", list);
                template = template.Replace("[TOTAL]", num.ToString());
                template = template.Replace("[TOTALAMOUNT]", totalAmount.ToString(DefaultKeys.AmountFormat));

                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4, true);
                results = WebHelpers.BuildResponse(pdfOutput, "Active Female Borrowers Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [Route("loanrequests")]
        public ResultObj LoanRequestsReport(LoanRequestsReportFilter filter)
        {
            ResultObj results;
            try
            {
                //var user = User.Identity.AsAppUser().Result;
                var db = new AppDbContext();
                //check period
                if (filter.StartDate == null) throw new Exception("Please specify the start date.");
                if (filter.EndDate == null) throw new Exception("Please specify the end date.");
                var sd = new DateTime(filter.StartDate.Value.Year, filter.StartDate.Value.Month, filter.StartDate.Value.Day, 0, 0, 0);
                var ed = new DateTime(filter.EndDate.Value.Year, filter.EndDate.Value.Month, filter.EndDate.Value.Day, 23, 59, 59);
                var records = filter.BuildQuery(new AppDbContext().LoanRequests).Include(x => x.User).ToList().Select(x => new
                {
                    x.Id,
                    x.Amount,
                    x.Rate,
                    x.ProcessingFee,
                    x.Days,
                    Name = x.User?.Name?.ToUpper(),
                    Gender = x.User?.Gender?.ToString().ToUpper(),
                    Email = x.User?.Email?.ToUpper(),
                    x.User?.DateOfBirth,
                    x.User?.UserType,
                    Nationality = x.User?.Nationality?.Name?.ToUpper(),
                    Status = x.Status.ToString().ToUpper(),
                    CreatedAt = x.CreatedAt.ToShortDateString()
                }).ToList();

                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/LoanRequests.html"));

                //Write company details
                template = template.Replace("[COMPANYNAME]", SetupConfig.Setting.CompanyDetails.Name?.ToUpper());
                template = template.Replace("[COMPANYADDRESS]", SetupConfig.Setting.CompanyDetails.Address?.ToUpper());
                template = template.Replace("[COMPANYLOGO]", SetupConfig.Setting.CompanyDetails.Logo);
                template = template.Replace("[STARTDATE]", sd.ToShortDateString());
                template = template.Replace("[ENDDATE]", ed.ToShortDateString());

                var list = string.Empty;
                var num = 0;

                foreach (var item in records)
                {
                    var dob = item.DateOfBirth ?? DateTime.Now;
                    var age = DateHelpers.GetAge(dob);
                    var ag = age > 0 ? age.ToString() : "";
                    num++;
                    list += "<tr style='border:none;font-size: 12px; text-transform: uppercase !important;'>" +
                            $"<td>{num}</td>" +
                            $"<td>{item.CreatedAt}</td>" +
                            $"<td>{item.Name}</td>" +
                            $"<td>{ag}</td>" +
                            $"<td>{item.Gender}</td>" +
                            $"<td class='text-right'>{item.Amount.ToString(DefaultKeys.AmountFormat)}</td>" +
                            $"<td class='text-right'>{(item.Rate / 100)}</td>" +
                            $"<td class='text-right'>{item.ProcessingFee.ToString(DefaultKeys.AmountFormat)}</td>" +
                            $"<td>{item.Days}</td>" +
                            $"<td>{item.Status}</td> </tr>";
                }
                var totalAmount = records.Sum(x => x.Amount);

                template = template.Replace("[LIST]", list);
                template = template.Replace("[TOTALAMOUNT]", totalAmount.ToString(DefaultKeys.AmountFormat));

                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4, true);
                results = WebHelpers.BuildResponse(pdfOutput, "Loan Requests Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [Route("loandisbursements")]
        public ResultObj LoanDisbursementsReport(LoanDisbursementsReportFilter filter)
        {
            ResultObj results;
            try
            {
                //var user = User.Identity.AsAppUser().Result;
                var db = new AppDbContext();
                //check period
                if (filter.StartDate == null) throw new Exception("Please specify the start date.");
                if (filter.EndDate == null) throw new Exception("Please specify the end date.");
                var sd = new DateTime(filter.StartDate.Value.Year, filter.StartDate.Value.Month, filter.StartDate.Value.Day, 0, 0, 0);
                var ed = new DateTime(filter.EndDate.Value.Year, filter.EndDate.Value.Month, filter.EndDate.Value.Day, 23, 59, 59);
                var records = filter.BuildQuery(new AppDbContext().LoanDisbursements).Include(x => x.Request.User).ToList().Select(x => new
                {
                    x.Id,
                    x.Amount,
                    x.ProcessingFee,
                    x.Request.Rate,
                    x.Request.Days,
                    Name = x.Request.User?.Name?.ToUpper(),
                    Gender = x.Request.User?.Gender?.ToString().ToUpper(),
                    Email = x.Request.User?.Email?.ToUpper(),
                    x.Request.User?.DateOfBirth,
                    x.Request.User?.UserType,
                    Nationality = x.Request.User?.Nationality?.Name?.ToUpper(),
                    Status = x.Status.ToString().ToUpper(),
                    CreatedAt = x.CreatedAt.ToShortDateString()
                }).ToList();

                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/LoanDisbursements.html"));

                //Write company details
                template = template.Replace("[COMPANYNAME]", SetupConfig.Setting.CompanyDetails.Name?.ToUpper());
                template = template.Replace("[COMPANYADDRESS]", SetupConfig.Setting.CompanyDetails.Address?.ToUpper());
                template = template.Replace("[COMPANYLOGO]", SetupConfig.Setting.CompanyDetails.Logo);
                template = template.Replace("[STARTDATE]", sd.ToShortDateString());
                template = template.Replace("[ENDDATE]", ed.ToShortDateString());

                var list = string.Empty;
                var num = 0;

                foreach (var item in records)
                {
                    var dob = item.DateOfBirth ?? DateTime.Now;
                    var age = DateHelpers.GetAge(dob);
                    var ag = age > 0 ? age.ToString() : "";
                    num++;
                    list += "<tr style='border:none;font-size: 12px; text-transform: uppercase !important;'>" +
                            $"<td>{num}</td>" +
                            $"<td>{item.CreatedAt}</td>" +
                            $"<td>{item.Name}</td>" +
                            $"<td>{ag}</td>" +
                            $"<td>{item.Gender}</td>" +
                            $"<td class='text-right'>{item.Amount.ToString(DefaultKeys.AmountFormat)}</td>" +
                            $"<td class='text-right'>{item.ProcessingFee.ToString(DefaultKeys.AmountFormat)}</td>" +
                            $"<td>{item.Status}</td> </tr>";
                }
                var totalAmount = records.Sum(x => x.Amount);

                template = template.Replace("[LIST]", list);
                template = template.Replace("[TOTALAMOUNT]", totalAmount.ToString(DefaultKeys.AmountFormat));

                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4, true);
                results = WebHelpers.BuildResponse(pdfOutput, "Loan Disbursements Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [Route("outstandingloans")]
        public ResultObj OutstandingLoansReport(LoansReportFilter1 filter)
        {
            ResultObj results;
            try
            {
                //var user = User.Identity.AsAppUser().Result;
                //check period
                if (filter.StartDate == null) throw new Exception("Please specify the start date.");
                if (filter.EndDate == null) throw new Exception("Please specify the end date.");
                var sd = new DateTime(filter.StartDate.Value.Year, filter.StartDate.Value.Month, filter.StartDate.Value.Day, 0, 0, 0);
                var ed = new DateTime(filter.EndDate.Value.Year, filter.EndDate.Value.Month, filter.EndDate.Value.Day, 23, 59, 59);
                var records = filter.BuildQuery(new AppDbContext().Loans.Where(x=> x.Status == LoanStatus.Active || x.Status == LoanStatus.Overdue)).ToList().Select(x => new
                {
                    x.Id,
                    Date = x.Date.ToShortDateString(),
                    x.AmountDisbursed,
                    x.InterestAmount,
                    x.TotalAmount,
                    x.AmountPaid,
                    x.AmountPayable,
                    NextRepaymentDate = x.NextRepaymentDate.ToShortDateString(),
                    RepaymentStartDate = x.RepaymentStartDate.ToShortDateString(),
                    x.PaidOn,
                    x.Disbursement.Amount,
                    x.Disbursement.ProcessingFee,
                    x.Disbursement.Request.Days,
                    Name = x.Disbursement.Request.User?.Name?.ToUpper(),
                    Gender = x.Disbursement.Request.User?.Gender?.ToString().ToUpper(),
                    Email = x.Disbursement.Request.User?.Email?.ToUpper(),
                    x.Disbursement.Request.User?.DateOfBirth,
                    x.Disbursement.Request.User?.UserType,
                    Status = x.Status.ToString().ToUpper(),
                }).ToList();

                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/OutstandingLoans.html"));

                //Write company details
                template = template.Replace("[COMPANYNAME]", SetupConfig.Setting.CompanyDetails.Name?.ToUpper());
                template = template.Replace("[COMPANYADDRESS]", SetupConfig.Setting.CompanyDetails.Address?.ToUpper());
                template = template.Replace("[COMPANYLOGO]", SetupConfig.Setting.CompanyDetails.Logo);
                template = template.Replace("[STARTDATE]", sd.ToShortDateString());
                template = template.Replace("[ENDDATE]", ed.ToShortDateString());

                var list = string.Empty;
                var num = 0;

                foreach (var item in records)
                {
                    var dob = item.DateOfBirth ?? DateTime.Now;
                    var age = DateHelpers.GetAge(dob);
                    var ag = age > 0 ? age.ToString() : "";
                    num++;
                    list += "<tr style='border:none;font-size: 12px; text-transform: uppercase !important;'>" +
                            $"<td>{num}</td>" +
                            $"<td>{item.Date}</td>" +
                            $"<td>{item.Name}</td>" +
                            $"<td>{ag}</td>" +
                            $"<td>{item.Gender}</td>" +
                            $"<td class='text-right'>{item.AmountDisbursed.ToString(DefaultKeys.AmountFormat)}</td>" +
                            $"<td class='text-right'>{item.InterestAmount.ToString(DefaultKeys.AmountFormat)}</td>" +
                            $"<td class='text-right'>{item.TotalAmount.ToString(DefaultKeys.AmountFormat)}</td>" +
                            $"<td class='text-right'>{item.AmountPaid.ToString(DefaultKeys.AmountFormat)}</td>" +
                            $"<td class='text-right'>{item.AmountPayable.ToString(DefaultKeys.AmountFormat)}</td></tr>";
                }
                var totalDisb = records.Sum(x => x.AmountDisbursed);
                var totalInt = records.Sum(x => x.InterestAmount);
                var totalPaid = records.Sum(x => x.AmountPaid);
                var totalOut = records.Sum(x => x.AmountPayable);
                var grandTotal = records.Sum(x => x.TotalAmount);

                template = template.Replace("[LIST]", list);
                template = template.Replace("[TOTALAMOUNTDISBURSED]", totalDisb.ToString(DefaultKeys.AmountFormat));
                template = template.Replace("[TOTALINTERESTAMOUNT]", totalInt.ToString(DefaultKeys.AmountFormat));
                template = template.Replace("[GRANDTOTALAMOUNT]", grandTotal.ToString(DefaultKeys.AmountFormat));
                template = template.Replace("[TOTALAMOUNTPAID]", totalPaid.ToString(DefaultKeys.AmountFormat));
                template = template.Replace("[TOTALAMOUNTOUTSTANDING]", totalOut.ToString(DefaultKeys.AmountFormat));

                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4, true);
                results = WebHelpers.BuildResponse(pdfOutput, "Outstanding Loans Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [Route("loanwriteoffs")]
        public ResultObj LoanWriteOffsReport(LoansReportFilter1 filter)
        {
            ResultObj results;
            try
            {
                //var user = User.Identity.AsAppUser().Result;
                //check period
                if (filter.StartDate == null) throw new Exception("Please specify the start date.");
                if (filter.EndDate == null) throw new Exception("Please specify the end date.");
                var sd = new DateTime(filter.StartDate.Value.Year, filter.StartDate.Value.Month, filter.StartDate.Value.Day, 0, 0, 0);
                var ed = new DateTime(filter.EndDate.Value.Year, filter.EndDate.Value.Month, filter.EndDate.Value.Day, 23, 59, 59);
                var records = filter.BuildQuery(new AppDbContext().Loans.Where(x => x.Status == LoanStatus.Cancelled)).ToList().Select(x => new
                {
                    x.Id,
                    Date = x.Date.ToShortDateString(),
                    x.AmountDisbursed,
                    x.InterestAmount,
                    x.TotalAmount,
                    x.AmountPaid,
                    x.AmountPayable,
                    NextRepaymentDate = x.NextRepaymentDate.ToShortDateString(),
                    RepaymentStartDate = x.RepaymentStartDate.ToShortDateString(),
                    x.PaidOn,
                    x.Disbursement.Amount,
                    x.Disbursement.ProcessingFee,
                    x.Disbursement.Request.Days,
                    Name = x.Disbursement.Request.User?.Name?.ToUpper(),
                    Gender = x.Disbursement.Request.User?.Gender?.ToString().ToUpper(),
                    Email = x.Disbursement.Request.User?.Email?.ToUpper(),
                    x.Disbursement.Request.User?.DateOfBirth,
                    x.Disbursement.Request.User?.UserType,
                    Status = x.Status.ToString().ToUpper(),
                }).ToList();

                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/LoansWriteOffs.html"));

                //Write company details
                template = template.Replace("[COMPANYNAME]", SetupConfig.Setting.CompanyDetails.Name?.ToUpper());
                template = template.Replace("[COMPANYADDRESS]", SetupConfig.Setting.CompanyDetails.Address?.ToUpper());
                template = template.Replace("[COMPANYLOGO]", SetupConfig.Setting.CompanyDetails.Logo);
                template = template.Replace("[STARTDATE]", sd.ToShortDateString());
                template = template.Replace("[ENDDATE]", ed.ToShortDateString());

                var list = string.Empty;
                var num = 0;

                foreach (var item in records)
                {
                    var dob = item.DateOfBirth ?? DateTime.Now;
                    var age = DateHelpers.GetAge(dob);
                    var ag = age > 0 ? age.ToString() : "";
                    num++;
                    list += "<tr style='border:none;font-size: 12px; text-transform: uppercase !important;'>" +
                            $"<td>{num}</td>" +
                            $"<td>{item.Date}</td>" +
                            $"<td>{item.Name}</td>" +
                            $"<td>{ag}</td>" +
                            $"<td>{item.Gender}</td>" +
                            $"<td class='text-right'>{item.AmountDisbursed.ToString(DefaultKeys.AmountFormat)}</td>" +
                            $"<td class='text-right'>{item.InterestAmount.ToString(DefaultKeys.AmountFormat)}</td>" +
                            $"<td class='text-right'>{item.TotalAmount.ToString(DefaultKeys.AmountFormat)}</td>" +
                            $"<td class='text-right'>{item.AmountPaid.ToString(DefaultKeys.AmountFormat)}</td>" +
                            $"<td class='text-right'>{item.AmountPayable.ToString(DefaultKeys.AmountFormat)}</td></tr>";
                }
                var totalDisb = records.Sum(x => x.AmountDisbursed);
                var totalInt = records.Sum(x => x.InterestAmount);
                var totalPaid = records.Sum(x => x.AmountPaid);
                var totalOut = records.Sum(x => x.AmountPayable);
                var grandTotal = records.Sum(x => x.TotalAmount);

                template = template.Replace("[LIST]", list);
                template = template.Replace("[TOTALAMOUNTDISBURSED]", totalDisb.ToString(DefaultKeys.AmountFormat));
                template = template.Replace("[TOTALINTERESTAMOUNT]", totalInt.ToString(DefaultKeys.AmountFormat));
                template = template.Replace("[GRANDTOTALAMOUNT]", grandTotal.ToString(DefaultKeys.AmountFormat));
                template = template.Replace("[TOTALAMOUNTPAID]", totalPaid.ToString(DefaultKeys.AmountFormat));
                template = template.Replace("[TOTALAMOUNTOUTSTANDING]", totalOut.ToString(DefaultKeys.AmountFormat));

                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4, true);
                results = WebHelpers.BuildResponse(pdfOutput, "Loan Write Offs Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [Route("overdueloans")]
        public ResultObj OverdueLoansReport(LoansReportFilter1 filter)
        {
            ResultObj results;
            try
            {
                //var user = User.Identity.AsAppUser().Result;
                //check period
                if (filter.StartDate == null) throw new Exception("Please specify the start date.");
                if (filter.EndDate == null) throw new Exception("Please specify the end date.");
                var sd = new DateTime(filter.StartDate.Value.Year, filter.StartDate.Value.Month, filter.StartDate.Value.Day, 0, 0, 0);
                var ed = new DateTime(filter.EndDate.Value.Year, filter.EndDate.Value.Month, filter.EndDate.Value.Day, 23, 59, 59);
                var today = DateTime.Now;
                var records = filter.BuildQuery(new AppDbContext().Loans.Where(x => (x.Status == LoanStatus.Overdue || x.Status == LoanStatus.Active) && x.NextRepaymentDate <= today)).ToList().Select(x => new
                {
                    x.Id,
                    Date = x.Date.ToShortDateString(),
                    x.AmountDisbursed,
                    x.InterestAmount,
                    x.TotalAmount,
                    x.AmountPaid,
                    x.AmountPayable,
                    NextRepaymentDate = x.NextRepaymentDate.ToShortDateString(),
                    RepaymentStartDate = x.RepaymentStartDate.ToShortDateString(),
                    x.PaidOn,
                    x.Disbursement.Amount,
                    x.Disbursement.ProcessingFee,
                    x.Disbursement.Request.Days,
                    Name = x.Disbursement.Request.User?.Name?.ToUpper(),
                    Gender = x.Disbursement.Request.User?.Gender?.ToString().ToUpper(),
                    Email = x.Disbursement.Request.User?.Email?.ToUpper(),
                    x.Disbursement.Request.User?.DateOfBirth,
                    x.Disbursement.Request.User?.UserType,
                    Status = x.Status.ToString().ToUpper(),
                }).ToList();

                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/OverdueLoans.html"));

                //Write company details
                template = template.Replace("[COMPANYNAME]", SetupConfig.Setting.CompanyDetails.Name?.ToUpper());
                template = template.Replace("[COMPANYADDRESS]", SetupConfig.Setting.CompanyDetails.Address?.ToUpper());
                template = template.Replace("[COMPANYLOGO]", SetupConfig.Setting.CompanyDetails.Logo);
                template = template.Replace("[STARTDATE]", sd.ToShortDateString());
                template = template.Replace("[ENDDATE]", ed.ToShortDateString());

                var list = string.Empty;
                var num = 0;

                foreach (var item in records)
                {
                    var dob = item.DateOfBirth ?? DateTime.Now;
                    var age = DateHelpers.GetAge(dob);
                    var ag = age > 0 ? age.ToString() : "";
                    num++;
                    list += "<tr style='border:none;font-size: 12px; text-transform: uppercase !important;'>" +
                            $"<td>{num}</td>" +
                            $"<td>{item.Date}</td>" +
                            $"<td>{item.Name}</td>" +
                            $"<td>{ag}</td>" +
                            $"<td>{item.Gender}</td>" +
                            $"<td class='text-right'>{item.AmountDisbursed.ToString(DefaultKeys.AmountFormat)}</td>" +
                            $"<td class='text-right'>{item.InterestAmount.ToString(DefaultKeys.AmountFormat)}</td>" +
                            $"<td class='text-right'>{item.TotalAmount.ToString(DefaultKeys.AmountFormat)}</td>" +
                            $"<td class='text-right'>{item.AmountPaid.ToString(DefaultKeys.AmountFormat)}</td>" +
                            $"<td class='text-right'>{item.AmountPayable.ToString(DefaultKeys.AmountFormat)}</td></tr>";
                }
                var totalDisb = records.Sum(x => x.AmountDisbursed);
                var totalInt = records.Sum(x => x.InterestAmount);
                var totalPaid = records.Sum(x => x.AmountPaid);
                var totalOut = records.Sum(x => x.AmountPayable);
                var grandTotal = records.Sum(x => x.TotalAmount);

                template = template.Replace("[LIST]", list);
                template = template.Replace("[TOTALAMOUNTDISBURSED]", totalDisb.ToString(DefaultKeys.AmountFormat));
                template = template.Replace("[TOTALINTERESTAMOUNT]", totalInt.ToString(DefaultKeys.AmountFormat));
                template = template.Replace("[GRANDTOTALAMOUNT]", grandTotal.ToString(DefaultKeys.AmountFormat));
                template = template.Replace("[TOTALAMOUNTPAID]", totalPaid.ToString(DefaultKeys.AmountFormat));
                template = template.Replace("[TOTALAMOUNTOUTSTANDING]", totalOut.ToString(DefaultKeys.AmountFormat));

                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4, true);
                results = WebHelpers.BuildResponse(pdfOutput, "Overdue Loans Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [Route("loanstatistics")]
        public ResultObj LoanStatictics(PeriodicReportsFilter filter)
        {
            ResultObj results;
            try
            {
                //var user = User.Identity.AsAppUser().Result;
                var db = new AppDbContext();
                //check period
                if (filter.StartDate == null) throw new Exception("Please specify the start date.");
                if (filter.EndDate == null) throw new Exception("Please specify the end date.");
                var sd = new DateTime(filter.StartDate.Value.Year, filter.StartDate.Value.Month, filter.StartDate.Value.Day, 0, 0, 0);
                var ed = new DateTime(filter.EndDate.Value.Year, filter.EndDate.Value.Month, filter.EndDate.Value.Day, 23, 59, 59);

                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/LoansStatistics.html"));

                //Write company details
                template = template.Replace("[COMPANYNAME]", SetupConfig.Setting.CompanyDetails.Name?.ToUpper());
                template = template.Replace("[COMPANYADDRESS]", SetupConfig.Setting.CompanyDetails.Address?.ToUpper());
                template = template.Replace("[COMPANYLOGO]", SetupConfig.Setting.CompanyDetails.Logo);
                template = template.Replace("[STARTDATE]", sd.ToShortDateString());
                template = template.Replace("[ENDDATE]", ed.ToShortDateString());

                //loan disbursements
                var loanDisbursements =
                    db.LoanDisbursements.Where(
                        x => x.Status == LoanDisbursementStatus.Paid && (x.ModifiedAt >= sd && x.ModifiedAt <= ed)).Include(x=> x.Request.User).ToList();
                var ldCount = loanDisbursements.Count();
                var ldValue = loanDisbursements.Sum(x => x.Amount);
                template = template.Replace("[NUMLOANSDISBURSED]", ldCount.ToString());
                template = template.Replace("[VALLOANSDISBURSED]", ldValue.ToString(DefaultKeys.AmountFormat));
                //loan disbursements to women
                var loanDisbursementToWomen = loanDisbursements.Where(x => x.Request.User.Gender == Gender.Female).ToList();
                var ldWomenCount = loanDisbursementToWomen.Count();
                var ldWomenValue = loanDisbursementToWomen.ToList().Sum(x => x.Amount);
                template = template.Replace("[NUMLOANSDISBURSEDWOMEN]", ldWomenCount.ToString());
                template = template.Replace("[VALLOANSDISBURSEDWOMEN]", ldWomenValue.ToString(DefaultKeys.AmountFormat));
                //loan disbursements to the youth
                var loanDisbursementToYouth = new List<LoanDisbursement>();
                foreach (var ld in loanDisbursements)
                {
                    var age = 0;
                    if (ld.Request.User.DateOfBirth != null) age = DateHelpers.GetAge((DateTime) ld.Request.User.DateOfBirth);
                    if (age >= SetupConfig.Setting.MinYouthAge && age <= SetupConfig.Setting.MaxYouthAge) loanDisbursementToYouth.Add(ld);
                }
                var ldYouthCount = loanDisbursementToYouth.Count();
                var ldYouthValue = loanDisbursementToYouth.Sum(x => x.Amount);
                template = template.Replace("[NUMLOANSDISBURSEDYOUTH]", ldYouthCount.ToString());
                template = template.Replace("[VALLOANSDISBURSEDYOUTH]", ldYouthValue.ToString(DefaultKeys.AmountFormat));
                //Interest
                var interestAmount = loanDisbursements.Sum(x => ((x.Request.Rate*x.Request.Amount)/100));
                template = template.Replace("[VALINTONLOANS]", interestAmount.ToString(DefaultKeys.AmountFormat));
                //Fees
                var totalFees = loanDisbursements.Sum(x => x.ProcessingFee);
                template = template.Replace("[VALFEESONLOANS]", totalFees.ToString(DefaultKeys.AmountFormat));
                //outstanding loans
                var outstandingLoans = db.Loans.Where(x => (x.Status == LoanStatus.Active || x.Status == LoanStatus.Overdue) && x.Date >= sd && x.Date <= ed && x.AmountPayable > 0).ToList();
                var olCount = outstandingLoans.Count();
                var olValue = outstandingLoans.Sum(x => x.AmountPayable);
                template = template.Replace("[NUMOUTSTANDINGLOANS]", olCount.ToString());
                template = template.Replace("[VALOUTSTANDINGLOANS]", olValue.ToString(DefaultKeys.AmountFormat));
                // loan write offs
                 var loanWriteOffs = db.Loans.Where(x => (x.Status == LoanStatus.Cancelled) && x.CancelledOn >= sd && x.CancelledOn <= ed && x.AmountPayable > 0).ToList();
                var lwoCount = loanWriteOffs.Count();
                var lwoValue = loanWriteOffs.Sum(x => x.AmountPayable);
                template = template.Replace("[NUMLOANWRITEOFFS]", lwoCount.ToString());
                template = template.Replace("[VALLOANWRITEOFFS]", lwoValue.ToString(DefaultKeys.AmountFormat));
                //Average loans disbursed
                var loans = db.Loans.Where(x => x.Date >= sd && x.Date <= ed).ToList();
                var avlCount = loans.Count();
                var avlValue = loans.Sum(x => x.AmountDisbursed);
                var averageLoans = avlValue/avlCount;
                template = template.Replace("[NUMAVELOANSDISBURSED]", avlCount.ToString());
                template = template.Replace("[VALAVELOANSDISBURSED]", averageLoans.ToString(DefaultKeys.AmountFormat));
                //Average loans outstanding
                var averageLoansOut = loans.Where(x=> x.AmountPayable > 0).ToList();
                var avoCount = averageLoansOut.Count();
                var avoValue = averageLoansOut.Sum(x => x.AmountDisbursed);
                var avLoansOut = avoValue / avoCount;
                template = template.Replace("[NUMAVELOANSOUTSTANDING]", avoCount.ToString());
                template = template.Replace("[VALAVELOANSOUTSTANDING]", avLoansOut.ToString(DefaultKeys.AmountFormat));
                //Top 10 largest loans
                var ttll = loans.OrderByDescending(x => x.AmountDisbursed).Take(10).ToList();
                var ttllValue = ttll.Sum(x => x.AmountDisbursed);
                template = template.Replace("[VALTOPTENLOANS]", ttllValue.ToString(DefaultKeys.AmountFormat));
                //Top 10 largest loans outstanding
                var ttllo = loans.OrderByDescending(x => x.AmountPayable).Take(10).ToList();
                var ttlloValue = ttllo.Sum(x => x.AmountPayable);
                template = template.Replace("[VALTOPTENLOANOUTSTANDING]", ttlloValue.ToString(DefaultKeys.AmountFormat));
                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4);
                results = WebHelpers.BuildResponse(pdfOutput, "Loan Statistics Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [Route("clientstatistics")]
        public ResultObj ClientStatistics(PeriodicReportsFilter filter)
        {
            ResultObj results;
            try
            {
                //var user = User.Identity.AsAppUser().Result;
                var db = new AppDbContext();
                //check period
                if (filter.StartDate == null) throw new Exception("Please specify the start date.");
                if (filter.EndDate == null) throw new Exception("Please specify the end date.");
                var sd = new DateTime(filter.StartDate.Value.Year, filter.StartDate.Value.Month, filter.StartDate.Value.Day, 0, 0, 0);
                var ed = new DateTime(filter.EndDate.Value.Year, filter.EndDate.Value.Month, filter.EndDate.Value.Day, 23, 59, 59);

                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/ClientStatistics.html"));

                //Write company details
                template = template.Replace("[COMPANYNAME]", SetupConfig.Setting.CompanyDetails.Name?.ToUpper());
                template = template.Replace("[COMPANYADDRESS]", SetupConfig.Setting.CompanyDetails.Address?.ToUpper());
                template = template.Replace("[COMPANYLOGO]", SetupConfig.Setting.CompanyDetails.Logo);
                template = template.Replace("[STARTDATE]", sd.ToShortDateString());
                template = template.Replace("[ENDDATE]", ed.ToShortDateString());
                //loan disbursements
                var loanDisbursements =
                    db.LoanDisbursements.Where(
                        x => x.Status == LoanDisbursementStatus.Paid && (x.ModifiedAt >= sd && x.ModifiedAt <= ed)).Include(x => x.Request.User).ToList();
                var ldCount = loanDisbursements.Count();
                template = template.Replace("[NUMLOANSDISBURSED]", ldCount.ToString());
                //loan disbursements to women
                var loanDisbursementToWomen = loanDisbursements.Where(x => x.Request.User.Gender == Gender.Female).ToList();
                var ldWomenCount = loanDisbursementToWomen.Count();
                template = template.Replace("[NUMLOANSDISBURSEDWOMEN]", ldWomenCount.ToString());
                //loan disbursements to the youth
                var loanDisbursementToYouth = new List<LoanDisbursement>();
                foreach (var ld in loanDisbursements)
                {
                    var age = 0;
                    if (ld.Request.User.DateOfBirth != null) age = DateHelpers.GetAge((DateTime)ld.Request.User.DateOfBirth);
                    if (age >= SetupConfig.Setting.MinYouthAge && age <= SetupConfig.Setting.MaxYouthAge) loanDisbursementToYouth.Add(ld);
                }
                var ldYouthCount = loanDisbursementToYouth.Count();
                template = template.Replace("[NUMLOANSDISBURSEDYOUTH]", ldYouthCount.ToString());
                
                //outstanding loans
                var outstandingLoans = db.Loans.Where(x => (x.Status == LoanStatus.Active || x.Status == LoanStatus.Overdue) && x.Date >= sd && x.Date <= ed && x.AmountPayable > 0).ToList();
                var olCount = outstandingLoans.Count();
                template = template.Replace("[NUMOUTSTANDINGLOANS]", olCount.ToString());
                // loan write offs
                var loanWriteOffs = db.Loans.Where(x => (x.Status == LoanStatus.Cancelled) && x.CancelledOn >= sd && x.CancelledOn <= ed && x.AmountPayable > 0).ToList();
                var lwoCount = loanWriteOffs.Count();
                template = template.Replace("[NUMLOANWRITEOFFS]", lwoCount.ToString());

                //New clients
                var clients =
                    db.Users.Where(x => x.CreatedAt >= sd && x.CreatedAt <= ed).OrderBy(x => x.CreatedAt).ToList();
                var newClientsCount = clients.Count();
                template = template.Replace("[NUMNEWCLIENTS]", newClientsCount.ToString());
                var femaleClients = clients.Where(x=> x.Gender == Gender.Female).ToList();
                var femaleClientsCount = femaleClients.Count();
                template = template.Replace("[NUMNEWFEMALECLIENTS]", femaleClientsCount.ToString());

                //New Borrowers



                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4);
                results = WebHelpers.BuildResponse(pdfOutput, "Client Statistics Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        //[Route("userslist/download")]
        //public ResultObj DownloadUsersList(UserFilter filter)
        //{
        //    ResultObj results;
        //    try
        //    {
        //        //var user = User.Identity.AsAppUser().Result;
        //        var users = filter.BuildQuery(new AppDbContext().Users).OrderBy(x => x.Name).ToList().Select(x => new
        //        {
        //            x.Id,
        //            x.UserType,
        //            Name = x.Name.ToUpper(),
        //            Gender = x.Gender?.ToString().ToUpper(),
        //            Email = x.Email?.ToUpper(),
        //            x.DateOfBirth,
        //            Nationality = x.Nationality?.Name?.ToUpper(),
        //            x.PhoneNumber,
        //            Status = x.Status.ToString().ToUpper()
        //        }).ToList();

        //        var sheetName = "UsersListReport";
        //        var ef = new ExcelFile();
        //        var ws = ef.Worksheets.Add(sheetName);
        //        var dt = new DataTable();
        //        dt.Columns.Add("USER TYPE", typeof(string));
        //        dt.Columns.Add("NAME", typeof(string));
        //        dt.Columns.Add("AGE", typeof(string));
        //        dt.Columns.Add("GENDER", typeof(string));
        //        dt.Columns.Add("PHONE NUMBER", typeof(string));
        //        dt.Columns.Add("NATIONALITY", typeof(string));
        //        dt.Columns.Add("STATUS", typeof(string));

        //        foreach (var user in users)
        //        {
        //            var dob = user.DateOfBirth ?? DateTime.Now;
        //            var age = DateHelpers.GetAge(dob);
        //            var ag = age > 0 ? age.ToString() : "";
        //            dt.Rows.Add(user.UserType, user.Name, ag,user.Gender, user.PhoneNumber,
        //                user.Nationality, user.Status);
        //        }
        //        ws.Cells.GetSubrangeAbsolute(0, 0, 0, 7).Merged = true;
        //        ws.Cells[0, 0].Style.HorizontalAlignment = HorizontalAlignmentStyle.Center;
        //        ws.Cells[0, 0].Style.Font.Weight = ExcelFont.BoldWeight;
        //        ws.Cells.GetSubrangeAbsolute(1, 0, 1, 7).Merged = true;
        //        ws.Cells[1, 0].Style.HorizontalAlignment = HorizontalAlignmentStyle.Center;
        //        ws.Cells[1, 0].Style.Font.Weight = ExcelFont.BoldWeight;

        //        ws.Cells[0, 0].Value = SetupConfig.Setting.CompanyDetails.Name?.ToUpper();
        //        ws.Cells[1, 0].Value = "USERS LIST REPORT";
        //        ws.InsertDataTable(dt,
        //            new InsertDataTableOptions
        //            {
        //                ColumnHeaders = true,
        //                StartRow = 3
        //            });
        //        var filename = sheetName + ".xlsx";
        //        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data/Downloads/" + filename);
        //        ef.Save(filePath);
        //        var bytes = File.ReadAllBytes(filePath);
        //        var file = Convert.ToBase64String(bytes);
        //        results = WebHelpers.BuildResponse(file, "Users List Report", true, 1);
        //    }
        //    catch (Exception ex)
        //    {
        //        results = WebHelpers.ProcessException(ex);
        //    }
        //    return results;
        //}
    }
}
