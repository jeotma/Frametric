export interface SelectionResultDto {
    movieId: string;
    title: string;
    directorName: string;
    releaseYear: number;
    selectionMechanismMetadata: string;
    posterUrl?: string | null;
    runtimeMinutes?: number | null;
}
