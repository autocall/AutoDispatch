using iTextSharp.text;
using iTextSharp.text.pdf;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Logic.Helpers.Dispatch;

namespace AutoDispatch.Web.Logic.Utils;

/// <summary>
///     PDF-операции над документами груза и трейлера (V1 DispatchPdfGenerator на iTextSharp 5 → iTextSharp.LGPLv2.Core, API 4.x):
///     подпись confirmation, склейка инвойса, штамп номера BNSF на схеме </summary>
public static class DispatchPdfGenerator {

    private static string _getConfirmedFileName(string fileName) {
        if (fileName.ToLower().Contains("_tmp")) {
            return fileName.ToLower().Replace("_tmp", "");
        }
        return "signed_" + fileName;
    }

    /// <summary>
    ///     Keywords PDF содержат позиции подписей в относительных координатах 0..1; Y > 1 — следующая страница </summary>
    private static void _signConfirmation(string inputFile, string outputFile, string signatureCarrier, string signatureAgent) {
        using Stream inputPdfStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        using Stream outputPdfStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None);

        var reader = new PdfReader(inputPdfStream);
        var stamper = new PdfStamper(reader, outputPdfStream);

        var keywordsValue = reader.Info["Keywords"]?.ToString() ?? "";
        Dictionary<string, string> keywords = keywordsValue.Split(';').Where(s => s.Contains('='))
            .ToDictionary(s => s.Split('=').First(), s => s.Split('=').Last());
        if (keywords.ContainsKey("carrier_signature") == false) {
            throw new Exception("Keywords doesn't have 'carrier_signature'");
        }
        if (keywords.ContainsKey("agent_signature") == false) {
            throw new Exception("Keywords doesn't have 'agent_signature'");
        }

        var carrierX = ParseFloat(keywords["carrier_signature"].Split(',').First());
        var carrierY = ParseFloat(keywords["carrier_signature"].Split(',').Last());
        var agentX = ParseFloat(keywords["agent_signature"].Split(',').First());
        var agentY = ParseFloat(keywords["agent_signature"].Split(',').Last());
        reader.Info["Keywords"] = null;

        var pageNumber = (int)carrierY + 1;

        Rectangle size = reader.GetPageSize(pageNumber);
        PdfContentByte pdfContentByte = stamper.GetOverContent(pageNumber);

        string fontPath = Path.Combine(AppSettings.FontsPath, "BRUSHSCI.TTF");
        BaseFont font = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
        pdfContentByte.SetColorFill(BaseColor.Black);
        pdfContentByte.SetFontAndSize(font, 16);

        var signatureHeight = 10;
        var signatureMarginWidth = 10;

        // adds carrier sign
        pdfContentByte.BeginText();
        pdfContentByte.ShowTextAligned(Element.ALIGN_LEFT, signatureCarrier ?? "",
            (float)(signatureMarginWidth + carrierX * size.Width),
            (float)(-signatureHeight + size.Height - carrierY % 1 * size.Height), 0);
        pdfContentByte.EndText();

        // adds agent sign
        pdfContentByte.BeginText();
        pdfContentByte.ShowTextAligned(Element.ALIGN_LEFT, signatureAgent ?? "",
            (float)(signatureMarginWidth + agentX * size.Width),
            (float)(-signatureHeight + size.Height - agentY % 1 * size.Height), 0);
        pdfContentByte.EndText();

        stamper.Close();
    }

    private static float ParseFloat(string value) => Single.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

    public static string SignConfirmation(LoadDto load, string signatureCarrier, string signatureAgent) {
        var confirmedFile = _getConfirmedFileName(load.ConfirmationFile);
        var confirmation_path = DispatchFileHelper.FileGetFullPath(load.EntityId, load.ConfirmationFile);
        var confirmed_path = DispatchFileHelper.FileGetFullPath(load.EntityId, confirmedFile);
        _signConfirmation(confirmation_path, confirmed_path, signatureCarrier, signatureAgent);
        return confirmedFile;
    }

    public static byte[] MergeDocs(List<string> filepaths) {
        using var document = new Document();
        using var newFileStream = new MemoryStream();
        var writer = new PdfCopy(document, newFileStream);
        document.Open();
        var readers = new List<PdfReader>();
        try {
            foreach (string fileName in filepaths) {
                var reader = new PdfReader(fileName);
                readers.Add(reader);
                reader.ConsolidateNamedDestinations();
                for (int i = 1; i <= reader.NumberOfPages; i++) {
                    writer.AddPage(writer.GetImportedPage(reader, i));
                }
            }
            writer.Close();
            document.Close();
            return newFileStream.ToArray();
        } finally {
            foreach (var reader in readers) {
                reader.Close();
            }
        }
    }

    public static void DeleteOriginScheme(TrailerDto trailerDto) {
        if (trailerDto.OriginSchemeFile != null) {
            var originFullPath = DispatchFileHelper.FileGetFullPath(trailerDto.EntityId, trailerDto.OriginSchemeFile);
            File.Delete(originFullPath);
            trailerDto.OriginSchemeFile = null;
        }
    }

    /// <summary>
    ///     Штамп «BNSF Pick-Up Number» на первой странице схемы; оригинал сохраняется как origin_* </summary>
    public static void StampScheme(TrailerDto trailerDto) {
        var sourceFullPath = DispatchFileHelper.FileGetFullPath(trailerDto.EntityId, trailerDto.SchemeFile);
        var originFullPath = DispatchFileHelper.FileGetFullPath(trailerDto.EntityId, "origin_" + trailerDto.SchemeFile);
        var tmpFullPath = DispatchFileHelper.FileGetFullPath(trailerDto.EntityId, "tmp_" + trailerDto.SchemeFile);

        if (trailerDto.OriginSchemeFile == null) {
            if (File.Exists(originFullPath) == false) {
                File.Copy(sourceFullPath, originFullPath);
            }
            trailerDto.OriginSchemeFile = Path.GetFileName(originFullPath);
        }
        File.Copy(originFullPath, tmpFullPath, true);

        var reader = new PdfReader(sourceFullPath);
        try {
            Rectangle size = reader.GetPageSizeWithRotation(1);
            using var stream = new FileStream(tmpFullPath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var document = new Document(size);
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            document.Open();
            {
                PdfContentByte cb = writer.DirectContent;
                PdfImportedPage page = writer.GetImportedPage(reader, 1);
                cb.AddTemplate(page, 0, 0);

                BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA_OBLIQUE, BaseFont.CP1252, false);
                cb.SetColorFill(BaseColor.Red);
                cb.SetFontAndSize(bf, 12);
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_RIGHT, $"BNSF Pick-Up Number: {trailerDto.BnsfPickUpNumber}", size.Width - 20, size.Height - 20, 0);
                cb.EndText();
            }
            for (int i = 1; i < reader.NumberOfPages; i++) {
                PdfImportedPage page = writer.GetImportedPage(reader, i + 1);
                document.NewPage();
                writer.DirectContent.AddTemplate(page, 0, 0);
            }
            document.Close();
        } finally {
            reader.Close();
        }
        File.Delete(sourceFullPath);
        File.Move(tmpFullPath, sourceFullPath);
    }
}
