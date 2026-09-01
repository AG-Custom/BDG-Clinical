using AG.CLINICAL.Application.Schedules.Dtos;
using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Schedules.AppointmentTags;

internal static class AppointmentTagsMapper
{
    public static AppointmentTagCatalogDto Map(TagAgendamento tag)
    {
        return new AppointmentTagCatalogDto(
            tag.Id,
            tag.Nome,
            tag.Cor,
            tag.Ativo,
            tag.CriadoEm,
            tag.AtualizadoEm);
    }

    public static IReadOnlyList<AppointmentTagCatalogDto> Map(IReadOnlyList<TagAgendamento> tags)
    {
        return tags.Select(Map).ToList();
    }
}
