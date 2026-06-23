export interface TmdbCollectionResultDto {
    id: number;
    name: string;
    overview: string | null;
    posterUrl: string | null;
    backdropUrl: string | null;
    parts: Array<TmdbCollectionPartDto>;
}

export interface TmdbCollectionPartDto {
    id: number;
    title: string;
    releaseDate: string | null;
    posterUrl: string | null;
    isInDatabase: boolean;
}
