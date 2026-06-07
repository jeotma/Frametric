export interface SlotReelResultDto {
    label: string;
    value: string;
}

export interface SlotMachineResultDto {
    movieId: string;
    title: string;
    directorName: string;
    releaseYear: number;
    posterUrl?: string | null;
    runtimeMinutes?: number | null;
    selectionMechanismMetadata: string;
    reelResults: SlotReelResultDto[];
    isJackpot: boolean;
}
