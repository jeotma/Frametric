// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;

namespace Frametric.Application.DTOs.Admin;

public record UserDto(
    Guid Id, 
    string Username, 
    string Email, 
    string Role,
    bool CanManageCatalog,
    bool CanAddUsers,
    bool CanDeleteUsers,
    bool CanPromoteToAdmin);
