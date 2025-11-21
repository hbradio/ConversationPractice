using System.IO.Pipelines;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

class Pdf
{
    public static string ReadPdf(string path)
    {
        var result = "";
        using (var pdf = PdfDocument.Open(path))
        {
            foreach (var page in pdf.GetPages())
            {
                var text = ContentOrderTextExtractor.GetText(page);
                result += text;
            }

        }
        return result;
    }
}