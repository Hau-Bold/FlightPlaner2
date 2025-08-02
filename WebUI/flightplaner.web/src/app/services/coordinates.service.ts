import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { GPS } from '../../Modules/gps.model';
import { HttpClient } from '@angular/common/http';
import { AppRoute } from '../../Modules/route.model';

@Injectable({
  providedIn: 'root'
})
export class CoordinatesService {

  private http = inject(HttpClient);

  constructor() { }

  public GetCoordinates(): Observable<AppRoute> {

      return this.http.get<GPS[]>('https://localhost:7182/api/GPS').pipe(
        map(coords => {
          const isFullRoute = coords.some(c => c.isStart);

          return { 
            coordinates: coords,
            isFullRoute: isFullRoute 
          } as AppRoute;
        })
      );
    }

    public applyOptimization(algorithm: string): Observable<AppRoute> {

    return this.http.get<GPS[]>(`https://localhost:7182/api/GPS/GetOptimizedCoordinates`, { params: { algorithm } })
    .pipe(map(optimized => {
       return { 
            coordinates: optimized, 
          } as AppRoute;
    }))
  }
}
