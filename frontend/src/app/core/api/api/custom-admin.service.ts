import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CustomAdminService {
  private basePath = '/api/admin';

  constructor(private http: HttpClient) {}

  public createUser(body: any): Observable<string> {
    return this.http.post<string>(`${this.basePath}/users`, body);
  }

  public demoteUser(userId: string): Observable<any> {
    return this.http.post<any>(`${this.basePath}/users/${userId}/demote`, {});
  }

  public deleteUser(userId: string): Observable<any> {
    return this.http.delete<any>(`${this.basePath}/users/${userId}`);
  }

  public updatePermissions(userId: string, body: any): Observable<any> {
    return this.http.post<any>(`${this.basePath}/users/${userId}/permissions`, body);
  }

  public updateMovie(movieId: string, body: any): Observable<any> {
    return this.http.post<any>(`${this.basePath}/catalog/movies/${movieId}`, body);
  }

  public updateActor(actorId: string, body: any): Observable<any> {
    return this.http.post<any>(`${this.basePath}/catalog/actors/${actorId}`, body);
  }

  public updateDirector(directorId: string, body: any): Observable<any> {
    return this.http.post<any>(`${this.basePath}/catalog/directors/${directorId}`, body);
  }

  public getRevisions(entityType: string, entityId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.basePath}/catalog/revisions/${entityType}/${entityId}`);
  }

  public restoreRevision(revisionId: string): Observable<any> {
    return this.http.post<any>(`${this.basePath}/catalog/revisions/${revisionId}/restore`, {});
  }

  public getUserViewingProfile(userId: string): Observable<any> {
    return this.http.get<any>(`${this.basePath}/users/${userId}/viewing-profile`);
  }
}
