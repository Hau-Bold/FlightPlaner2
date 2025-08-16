import { Component } from '@angular/core';
import { FormGroup, FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { GPS } from '../../Modules/gps.model';
import { CommonModule } from '@angular/common';
import { Inject } from '@angular/core';
import { CoordinatesService } from '../services/coordinates.service';
import { CoordinateRequest } from '../../Modules/coordinateRequest.model';

@Component({
  standalone: true,
  selector: 'app-route-planning',
  templateUrl: './route-planning.component.html',
  styleUrls: ['./route-planning.component.css'],
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
})
export class RoutePlanningComponent {
  coordinatesArray: GPS[] = [];
  isFullRoute = false;
  optimizedCoordinates: GPS[] = []; 

  constructor( @Inject(CoordinatesService) private readonly coordinatesService: CoordinatesService) { 
    
    this.coordinatesService.GetCoordinates().subscribe((route) => {
      this.coordinatesArray = route.coordinates; 
      this.isFullRoute=route.isFullRoute;
    });
  }
 
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
  } as CoordinateRequest;

  console.log("start: " + coordinateRequest.isStart);

  this.coordinatesService.Post(coordinateRequest).subscribe({
    next: (value) => {
      console.log(value);
      //refresh
      this.coordinatesService.GetCoordinates().subscribe((route) => {
      this.coordinatesArray = route.coordinates; 
      this.isFullRoute=route.isFullRoute;
    });
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

public onDelete(id: string): void {
  this.coordinatesService.Delete(id)
    .subscribe(
      {
        next:
           (value) => {
           console.log(value);
           alert('item deleted');
           //refresh
           this.coordinatesService.GetCoordinates().subscribe(
            (route) => {
                        this.coordinatesArray = route.coordinates; 
                        this.isFullRoute=route.isFullRoute;
                        });
                      },
          error:(err) =>{
            console.error(err);
          }            
       });
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
   this.coordinatesService.applyOptimization(algorithm)
   .subscribe(route=> this.optimizedCoordinates=route.coordinates);
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

public GetPredecessor(index: number, coordinates: GPS[]): GPS | null
{
  if (index > 0 && index < coordinates.length) {
    return coordinates[index - 1];
  }
  return null;
}

public TotalDistance(cordinates:GPS[]):number
{

let result: number=0;
let index: number =0;

  for(index=0; index < cordinates.length - 2; index++) {
    const from = cordinates[index];
    const to = cordinates[index + 1];
    result += this.GetDistanceBetween(from, to);
  }
  return this.roundToDecimals(result,2);;
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
}
