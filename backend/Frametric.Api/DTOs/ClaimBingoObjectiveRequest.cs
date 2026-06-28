using System;

namespace Frametric.Api.DTOs;

public record ClaimBingoObjectiveRequest(
    Guid ObjectiveId,
    Guid DiaryEntryId
);
