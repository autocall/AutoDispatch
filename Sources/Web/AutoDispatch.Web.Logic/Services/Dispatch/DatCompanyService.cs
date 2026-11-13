using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Email;
using AutoDispatch.Web.Logic.WebServices;
using AutoDispatch.Web.Logic.WebServices.Safer;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

/// <summary>
///     Карриеры и брокеры (V1 DatCompanyService): карточка FMCSA через Safer с запасным safer.fmcsa.dot.gov, тестовые письма SMTP компании </summary>
public class DatCompanyService : DispatchCrudService<DatCompanyDto, DatCompany> {

    public DatCompanyService(ServicesHost host) : base(host) { }

    /// <summary>
    ///     Gets company details by MC# or DOT# </summary>
    public async Task<Dictionary<string, string>> GetFieldsAsync(int? dot, int? mc) {
        var result = await Host.GetService<BrokersnapshotWebService>().CompanyAsync(dot, mc);
        if (result.Success) {
            return result.Data.CreateFields();
        }
        _l.e($"GetFields: {result.ErrorMessage}");
        var safer = Host.GetService<SaferFmcsaWebService>();
        ApiResult<Dictionary<string, string>> fallback;
        if (mc != null) {
            fallback = await safer.GetFieldsAsync(mc.Value, true);
        } else if (dot != null) {
            fallback = await safer.GetFieldsAsync(dot.Value, false);
        } else {
            throw new InfoException("Undefined DOT & MC");
        }
        if (fallback.Success == false) {
            throw new InfoException(fallback.ErrorMessage);
        }
        return fallback.Data;
    }

    public async Task SendTestEmail(ActualCompanyDto webCompany, string email) {
        if (email == null) throw new InfoException("email is null");
        var dbCompany = await GetByIdAsync<DispatchCompany>(webCompany.EntityId);
        var smtpSettings = new SmtpSettings(
            webCompany.SMTP_Email,
            webCompany.SMTP_Password ?? dbCompany.SMTP_Password,
            webCompany.SMTP_Host,
            webCompany.SMTP_Port);
        var emailObj = new Email.Email(email, webCompany.SMTP_Email, "Test", "Test");
        await EmailUtils.SendEmailAsync(smtpSettings, emailObj);
    }

    public async Task SendTestSchemeEmail(ActualCompanyDto webCompany, string email) {
        var dbCompany = await GetByIdAsync<DispatchCompany>(webCompany.EntityId);
        var smtpSettings = new SmtpSettings(
            webCompany.SMTP_SchemeEmail,
            webCompany.SMTP_SchemePassword ?? dbCompany.SMTP_SchemePassword,
            webCompany.SMTP_SchemeHost,
            webCompany.SMTP_SchemePort);
        var emailObj = new Email.Email(email, webCompany.SMTP_SchemeEmail, "Test", "Test Scheme");
        await EmailUtils.SendEmailAsync(smtpSettings, emailObj);
    }
}
