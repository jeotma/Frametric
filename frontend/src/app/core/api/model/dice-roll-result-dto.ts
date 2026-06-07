export interface SingleDieResultDto {
    diceType: number;
    rollValue: number;
    label: string;
    description: string;
}

export interface DiceRollResultDto {
    movieId: string;
    title: string;
    directorName: string;
    releaseYear: number;
    posterUrl?: string | null;
    runtimeMinutes?: number | null;
    selectionMechanismMetadata: string;
    diceResults: SingleDieResultDto[];
    specialEvent?: string | null;
}
