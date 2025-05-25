import { Component,inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { provideHttpClient } from '@angular/common/http'; //since the old way is deprecated
import { HttpClient } from '@angular/common/http';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { GPS } from '../../Modules/gps.model';
import { BehaviorSubject, firstValueFrom, Observable,tap } from 'rxjs';

@Component({
  selector: 'app-route-planning',
  templateUrl: './route-planning.component.html',
  styleUrl: './route-planning.component.css'
})
export class RoutePlanningComponent {

  private optimizedCoordinatesSubject = new BehaviorSubject<GPS[]>([]);
  optimizedCoordinates$ = this.optimizedCoordinatesSubject.asObservable();

  coordinatesArray: GPS[] = [];
  isFullRoute = false;
  optimizedCoordinates: GPS[] = []; 

  constructor(private router: Router) {
  }
  http = inject(HttpClient);

  coordinatesForm = new FormGroup(
    {
      street: new FormControl<string | null>(null),
      city: new FormControl<string>(''),
      postalCode: new FormControl<string | null>(null),
      country: new FormControl<string>(''),
      isStart: new FormControl<boolean>(false),
    });

 async onFormSubmit() {
  const coordinateRequest = {
    street: this.coordinatesForm.value.street,
    city: this.coordinatesForm.value.city,
    postalCode: this.coordinatesForm.value.postalCode,
    country: this.coordinatesForm.value.country,
    isStart: this.coordinatesForm.value.isStart ?? false
  };

  console.log("start: " + coordinateRequest.isStart);

  this.http.post('https://localhost:7182/api/GPS', coordinateRequest).subscribe({
    next: (value) => {
      console.log(value);
      this.coordinates$ = this.GetCoordinates(); // Refresh values
      this.coordinatesForm.reset();         
    },
    error: (err) => {
      if (err.status === 409) {
        const resetForm = window.confirm("This coordinate is already inserted. Do you want to reset the form?");
        if (resetForm) {
          this.coordinatesForm.reset();
        }
      } else {
        alert("An error occurred while submitting the coordinate: {err.status}");
      }
    }
  });
}

  coordinates$ = this.GetCoordinates();

  public onDelete(id: string): void {
    this.http.delete(`https://localhost:7182/api/GPS/${id}`)
      .subscribe(
        {
          next: (value) => {
            console.log(value);
            alert('item deleted');
            this.coordinates$ = this.GetCoordinates();
          }
        }
      );
  }

  public onEdit(gps: GPS): void {

    if (confirm("item will be deleted and newly created")) {
      this.coordinatesForm.controls['city'].setValue(gps.city);
      this.coordinatesForm.controls['country'].setValue(gps.country);
      this.coordinatesForm.controls['street'].setValue(gps.street);
      this.coordinatesForm.controls['postalCode'].setValue(gps.postalCode);
      this.coordinatesForm.controls['isStart'].setValue(gps.isStart);

      this.onDelete(gps.guid);
    }
  }

  public applyOptimization(algorithm: string): void {
    this.http.get<GPS[]>(`https://localhost:7182/api/GPS/GetOptimizedCoordinates`, { params: { algorithm } })
      .subscribe({
        next: (optimized) => {
          this.optimizedCoordinatesSubject.next(optimized); // Update optimized coordinates
        },
        error: (err) => console.error('Optimization failed', err),
      });
  }

  public GetDisplayName(gps: GPS): string {

    var displayName = [];
    displayName.push(gps.city);
    displayName.push(gps.country);
    if (gps.street != '') {
      displayName.push(gps.street);
    }
    if (gps.postalCode != '') {
      displayName.push(gps.postalCode);
    }

    return displayName.toString();
  }

  private GetCoordinates(): Observable<GPS[]> {
    return this.http.get<GPS[]>('https://localhost:7182/api/GPS').pipe(
      tap(coords => {
        this.coordinatesArray = coords;
        this.isFullRoute = coords.some(c => c.isStart);
      })
    );
  }

 public GetPredecessor(index: number): GPS | null {
    if (index > 0 && index < this.coordinatesArray.length) {
      return this.coordinatesArray[index - 1];
    }
    return null;
  }

  public GetDistanceBetween(from: GPS, to: GPS): number
  {
    const radius = 6371; 

    const lonFrom = this.toRadians(Number(from.lon));
    const latFrom = this.toRadians(Number(from.lat));

    const lonTo = this.toRadians(Number(to.lon));
    const latTo = this.toRadians(Number(to.lat));

    let distance = Math.sin(latFrom) * Math.sin(latTo);
    distance += Math.cos(latFrom) * Math.cos(latTo) * Math.cos(lonTo - lonFrom);
    distance = Math.acos(distance);
    distance *= radius;

    return this.roundToDecimals(distance,2);
  }

  private roundToDecimals(value: number, decimals: number): number {
    const factor = Math.pow(10, decimals);
    return Math.round(value * factor) / factor;
  }
  
  private toRadians(degrees: number): number {
    return degrees * (Math.PI / 180);
  }

  public onAnimate() {
    window.open('./animate', '_blank', 'location=yes,height=570,width=520,scrollbars=yes,status=yes')
  }

  navigate() {
    console.log('trying to navigate');
    this.router.navigateByUrl("C:\\Users\\user\\source\\repos\\FlightPlaner\\UI\\flightplaner.web\\src\\app\\animation\\animation.component.html");
  }

}
