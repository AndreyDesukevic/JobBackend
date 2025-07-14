using System.Text;

namespace JobMonitor.Domain.Dto;

public class ResumeAggregatedInfoDto
{
    public string Summary { get; set; } = string.Empty;
    public string CombinedExperienceDescription { get; set; } = string.Empty;
    public string CombinedSkills { get; set; } = string.Empty;

    public string ToPlainText()
    {
        var builder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(Summary))
        {
            builder.AppendLine("Описание:");
            builder.AppendLine(Summary);
            builder.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(CombinedExperienceDescription))
        {
            builder.AppendLine("Опыт работы:");
            builder.AppendLine(CombinedExperienceDescription);
            builder.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(CombinedSkills))
        {
            builder.AppendLine("Навыки:");
            builder.AppendLine(CombinedSkills);
        }

        return builder.ToString().TrimEnd();
    }
}