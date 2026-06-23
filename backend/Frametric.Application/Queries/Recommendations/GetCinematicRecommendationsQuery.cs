// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Frametric.Application.DTOs;
using Frametric.Domain.Enums;
using MediatR;

namespace Frametric.Application.Queries.Recommendations;

public record GetCinematicRecommendationsQuery(
    Guid UserId,
    RecommendationStrategy Strategy,
    RecommendationScope Scope,
    int Quantity,
    int? MaxRuntimeMinutes = null,
    int? MinRuntimeMinutes = null
) : IRequest<IEnumerable<RecommendedMovieDto>>;

public class GetCinematicRecommendationsValidator : AbstractValidator<GetCinematicRecommendationsQuery>
{
    private static readonly int[] AllowedQuantities = [1, 2, 3, 5, 10];

    public GetCinematicRecommendationsValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Strategy).IsInEnum();
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.Quantity)
            .Must(q => AllowedQuantities.Contains(q))
            .WithMessage("You can only request 1, 2, 3, 5, or 10 recommendations at a time.");
    }
}
