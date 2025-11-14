import { inject, Injectable, signal } from '@angular/core';
import { firstValueFrom, map, Observable, of } from 'rxjs';
import { GPS } from '../../Modules/gps.model';
import { HttpClient } from '@angular/common/http';
import { AppRoute } from '../../Modules/route.model';
import { Point } from '../../Modules/point.model';
import { CoordinateRequest } from '../../Modules/coordinateRequest.model';

@Injectable({
  providedIn: 'root'
})
export class CoordinatesService {

  private http = inject(HttpClient);
  // private optimizedCoordinates: GPS[] | null = null;
  private optimizedCoordinates= signal<GPS[]|null>(null);
  

  constructor() {}

  // public GetCoordinates(): Observable<AppRoute> {
  //   return this.http.get<GPS[]>('https://localhost:7182/api/GPS').pipe(
  //     map(coords => ({
  //       coordinates: coords,
  //       isFullRoute: coords.some(c => c.isStart)
  //     }))
  //   );
  // }

  private coordinates= signal<GPS[]|null>(null);
   public async GetCoordinates(): Promise<AppRoute> {

    const data = await firstValueFrom( this.http.get<GPS[]>('https://localhost:7182/api/GPS'));
    this.coordinates.set(data);
    
    return{
      coordinates: this.coordinates() ?? [],
      isFullRoute: data.some(c => c.isStart)
    } as AppRoute;
  }

  // public applyOptimization(algorithm: string): Observable<AppRoute> {
  //   return this.http.get<GPS[]>(`https://localhost:7182/api/GPS/GetOptimizedCoordinates`, { params: { algorithm } }).pipe(
  //     map(optimized => {
  //       this.optimizedCoordinates = optimized;
  //       return {
  //         coordinates: optimized
  //       } as AppRoute;
  //     })
  //   );
  // }

  public async applyOptimization(algorithm: string): Promise<AppRoute> {
    const optimized = await firstValueFrom(
      this.http.get<GPS[]>('https://localhost:7182/api/GPS/GetOptimizedCoordinates', { params: { algorithm } })
    );
    this.optimizedCoordinates.set(optimized);
    return { coordinates: optimized } as AppRoute;
  }

public Delete(id:string):Observable<Object>
{
    return this.http.delete(`https://localhost:7182/api/GPS/${id}`)
}

public Post(coordinateRequest:CoordinateRequest):Observable<Object>
{
  return this.http.post('https://localhost:7182/api/GPS', coordinateRequest)
}

// public GetMercatorCoordinates(
//     actualWidth: number,
//     actualHeight: number,
//     imageWidth: number,
//     imageHeight: number
//   ): Observable<Point[]> {
//     const data$: Observable<GPS[]> =
//       this.optimizedCoordinates
//         ? of(this.optimizedCoordinates)
//         : this.http.get<GPS[]>('https://localhost:7182/api/GPS');

//     return data$.pipe(
//       map(coords =>
//         coords.map(c =>
//           this.ToMercator(c, actualWidth, actualHeight, imageWidth, imageHeight)
//         )
//       )
//     );
//   }

public GetMercatorCoordinates( actualWidth: number,  actualHeight: number, imageWidth: number,  imageHeight: number ):Point[]
   {
    const data = this.optimizedCoordinates() ?? [];
    return data.map(c => this.ToMercator(c, actualWidth, actualHeight, imageWidth, imageHeight));
   }

  private ToMercator(
    gps: GPS,
    actualWidth: number,
    actualHeight: number,
    imageWidth: number,
    imageHeight: number
  ): Point {
    const lon: number = Number(gps.lon);
    let lat: number = Number(gps.lat);

    lat = Math.max(-89.5, Math.min(89.5, lat)); // Clamp lat

    const latRad = lat * Math.PI / 180;
    const xMercator = (lon + 180.0) / 360.0 * imageWidth;
    const yMercator =
      imageHeight / 2.0 -
      (imageWidth / (2.0 * Math.PI)) * Math.log(Math.tan(Math.PI / 4.0 + latRad / 2.0));

    const xPx = Math.trunc(xMercator * (actualWidth / imageWidth));
    const yPx = Math.trunc(yMercator * (actualHeight / imageHeight));

    return {
      xPx,
      yPx,
      isStart: gps.isStart,
      city: gps.city
    };
  }
}
