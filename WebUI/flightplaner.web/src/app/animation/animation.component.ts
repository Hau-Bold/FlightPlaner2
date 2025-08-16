import { AfterViewInit, Component, ElementRef, Inject, inject, ViewChild } from '@angular/core';
import { TargetAnimationState } from '../../Modules/targetAnimationState.model';
import { Point } from '../../Modules/point.model';
import { firstValueFrom } from 'rxjs';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CoordinatesService } from '../services/coordinates.service';

@Component({
  standalone: true,
  selector: 'app-animation',
  templateUrl: './animation.component.html',
  styleUrls: ['./animation.component.css'],
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
})
export class AnimationComponent implements AfterViewInit {
  private targetStates: { point: Point; state: TargetAnimationState }[] = [];
  private mercatorCoordinates: Point[] = [];
  private backgroundImageUrl = '../../assets/M1.jpg';
  private backgroundImageUrl2 = '../../assets/M2.jpg';
  private planeImagePath='../../assets/final-Photoroom.png';
  private plane: HTMLImageElement;
  private toggleBackground: boolean = true;
  public showRoute: boolean = false;
  public routeClickLabel: string = 'Route';
  public flightLabel:string ='Start';
  private currentPlaneLocation: Point = { xPx: 0, yPx: 0, city: '' ,isStart:false};
  private targetsIndex: number=1;

  radius = 10;
  offset = 2;
  private animationId: number | null = null;
  private offscreenCanvas: HTMLCanvasElement;
  private offscreenContext: CanvasRenderingContext2D | null;
  public isAnimating: boolean = false;
  public isFlying:boolean = false;
  public imgWidth: number = 0;
  public imgHeight: number = 0;

  @ViewChild('canvas') myCanvas!: ElementRef;
  background: HTMLImageElement = new Image();

  constructor( @Inject(CoordinatesService) private readonly coordinatesService: CoordinatesService) {
    this.offscreenCanvas = document.createElement('canvas');
    this.offscreenContext = this.offscreenCanvas.getContext('2d');
    this.plane = new Image();
    this.plane .src = this.planeImagePath;
  }

  ngAfterViewInit(): void {
    this.processImage();
   
  }

  processImage() {
    const canvas: HTMLCanvasElement = this.myCanvas.nativeElement;
    this.fitToContainer(canvas);

    this.offscreenCanvas.width = canvas.width;
    this.offscreenCanvas.height = canvas.height;

    this.background = new Image();
    this.background.src = this.toggleBackground ? this.backgroundImageUrl : this.backgroundImageUrl2;

    this.background.onload = () => {
      if (this.offscreenContext) {
        this.offscreenContext.drawImage(this.background, 0, 0, canvas.width, canvas.height);
        const context = canvas.getContext('2d');
        if (context) {
          context.drawImage(this.offscreenCanvas, 0, 0);

          this.imgWidth = this.offscreenCanvas.width;
          this.imgHeight = this.offscreenCanvas.height;

this.coordinatesService.GetMercatorCoordinates(canvas.width, canvas.height, this.background.width, this.background.height)
            .subscribe(points => {
              this.mercatorCoordinates = points;
              this.drawNeedles(this.mercatorCoordinates);
               this.currentPlaneLocation = this.mercatorCoordinates[0];
              this.targetsIndex=1;
            });
             
        }
      }
    };
  }

  fitToContainer(canvas: HTMLCanvasElement) {
    canvas.style.width = '100%';
    canvas.style.height = '100%';
    canvas.width = canvas.offsetWidth;
    canvas.height = canvas.offsetHeight;
  }

  public animateTargets(): void {
    const canvas = this.myCanvas.nativeElement;
    const actualWidth = canvas.width;
    const actualHeight = canvas.height;
    const context = canvas.getContext('2d');

    this.isAnimating = !this.isAnimating;

    const elem = document.getElementById("AnimateTargets");
    if (elem) {
      elem.innerHTML = this.isAnimating ? "Stop" : "AnimateTargets";
    }

    if (this.isAnimating) {

      this.coordinatesService.GetMercatorCoordinates(actualWidth, actualHeight, this.background.width, this.background.height)
        .subscribe(millerCoordinates => {
          this.targetStates = millerCoordinates.map(point => ({
            point,
            state: { counter: 0, radius: 10 }
          }));

          if (this.offscreenCanvas && context) {
            context.clearRect(0, 0, canvas.width, canvas.height);
            context.drawImage(this.offscreenCanvas, 0, 0);
          }

          this.startAnimationLoop();
        });


        
    } else {
      this.stopAnimation();
    }
  }

private async startAnimationLoop(): Promise<void> {
  if (this.animationId) 
  {
    return;
  }

  const canvas = this.myCanvas.nativeElement;
  const context = canvas.getContext('2d');
  if (!context) return;

  // Wait for the projected route points ONCE before animation starts
  const routePoints = this.showRoute
    ? await firstValueFrom(
        this.coordinatesService.GetMercatorCoordinates(
          canvas.width,
          canvas.height,
          this.background.width,
          this.background.height
        )
      )
    : [];

  let lastRenderTime = 0;
  const frameInterval = 150;

  const animateFrame = (timestamp: number) => {
    if (!this.isAnimating && !this.isFlying) 
      {
        return;
      }

    const elapsed = timestamp - lastRenderTime;
    if (elapsed >= frameInterval) {
      lastRenderTime = timestamp;

      context.clearRect(0, 0, canvas.width, canvas.height);
      context.drawImage(this.offscreenCanvas, 0, 0);

      // Draw route once points are available
      if (this.showRoute && routePoints.length > 1) {
        this.drawRoute(context, routePoints);
      }

     // move plane
      if (this.isFlying && this.targetsIndex < this.mercatorCoordinates.length) {
        const isMoving = this.movePlaneto(this.mercatorCoordinates[this.targetsIndex], context);
        if (!isMoving) this.targetsIndex++;
      }

      // Animate targets
      this.targetStates.forEach(({ point, state }) => {
        context.beginPath();
        context.arc(point.xPx, point.yPx, state.radius, 0, 2 * Math.PI);
        context.strokeStyle = 'red';
        context.stroke();

        context.font = '12px Arial';
        context.fillStyle = this.toggleBackground ? 'white' : 'black';
        context.textAlign = 'center';
        context.fillText(point.city, point.xPx, point.yPx - state.radius - 5);

        state.radius += this.offset;
        state.counter++;

        if (state.counter >= 10) {
          state.radius = this.radius;
          state.counter = 0;
        }
      });
    }

    this.animationId = requestAnimationFrame(animateFrame);
  };

  this.animationId = requestAnimationFrame(animateFrame);
}

private movePlaneto(target: Point,context: CanvasRenderingContext2D): boolean
{
console.log('move plae to called:');

const dx = target.xPx - this.currentPlaneLocation.xPx;
const dy = target.yPx - this.currentPlaneLocation.yPx;
 
const distance = Math.sqrt(dx * dx + dy * dy);

if(distance < 1) {
  return false; 
}

// calc new position:

if (dx > 0) {
  this.currentPlaneLocation.xPx += 1;
} else if (dx < 0) {
  this.currentPlaneLocation.xPx -= 1;
}

if (dy > 0) {
  this.currentPlaneLocation.yPx += 1;
} else if (dy < 0) {
  this.currentPlaneLocation.yPx -= 1;
}

const angle =this.getAngleBetweenPoints(this.currentPlaneLocation,target);

// draw at (new) current posdition
this.drawRotatedPlane(context,this.plane,this.currentPlaneLocation.xPx,this.currentPlaneLocation.yPx, angle);

return true;
}

  public stopAnimation(): void {
    this.isAnimating = false;
    if (this.animationId) {
      cancelAnimationFrame(this.animationId);
      this.animationId = null;
    }

    this.targetStates = [];

    requestAnimationFrame(() => {
      const canvas = this.myCanvas.nativeElement;
      const context = canvas.getContext('2d');
      if (context) {
        context.clearRect(0, 0, canvas.width, canvas.height);
        context.drawImage(this.offscreenCanvas, 0, 0);

        if (this.showRoute) {
          this.drawRoute(context, this.mercatorCoordinates);
        }

        this.drawNeedles(this.mercatorCoordinates);
      }
    });
  }

  public toggleCard(): void {
    this.toggleBackground = !this.toggleBackground;
    this.processImage();
  }

  public Route(): void {
    this.showRoute = !this.showRoute;
    this.routeClickLabel = this.showRoute? 'Hide': 'Route';

    if (!this.isAnimating) {
      const canvas = this.myCanvas.nativeElement;
      const context = canvas.getContext('2d');
      if (context) {
        context.clearRect(0, 0, canvas.width, canvas.height);
        context.drawImage(this.offscreenCanvas, 0, 0);

        if (this.showRoute) {
          this.drawRoute(context, this.mercatorCoordinates);
        }

        this.drawNeedles(this.mercatorCoordinates);
      }
    }
  }

public Fly(): void {
  this.isFlying = !this.isFlying;

  if (this.isFlying && !this.animationId) {
    this.startAnimationLoop();
  }

  this.flightLabel = this.isFlying ? "Stop" : "Start";
}
    

  private drawRoute(context: CanvasRenderingContext2D, points: Point[]): void {
    if (!points || points.length < 2) return;

    context.beginPath();
    context.moveTo(points[0].xPx, points[0].yPx);
    for (const point of points) {
      context.lineTo(point.xPx, point.yPx);
    }
    context.strokeStyle = this.toggleBackground ? 'white' : 'black';
    context.setLineDash([4, 3]);
    context.lineWidth = 1;
    context.stroke();

    context.setLineDash([]);
  }

  private drawNeedles(points: Point[]): void {
    const canvas = this.myCanvas.nativeElement;
    const context = canvas.getContext('2d');
    if (!context)
    {
       return;
    }

    points.forEach(point => {
      context.beginPath();
      context.moveTo(point.xPx, point.yPx);
      context.lineTo(point.xPx - 4, point.yPx + 10);
      context.lineTo(point.xPx + 4, point.yPx + 10);
      context.closePath();
      context.fillStyle = 'red';
      context.fill();

      context.font = '12px Arial';
      context.fillStyle = this.toggleBackground ? 'white' : 'black';
      context.textAlign = 'center';
      context.fillText(point.city, point.xPx, point.yPx - 8);
    });
  }

 

private drawRotatedPlane(
  context: CanvasRenderingContext2D,
  img: HTMLImageElement,
  x: number,
  y: number,
  angle: number,
  displayWidth: number = 50,
  displayHeight: number = 50
) {
  context.save();
  context.translate(x, y);
  context.rotate(angle);
  // Draw scaled image centered at (0,0)
  context.drawImage(img, -displayWidth / 2, -displayHeight / 2, displayWidth, displayHeight);
  context.restore();

  console.log('Drawing plane at:', x, y, 'with angle:', angle);
}
  private drawPlane(point: Point, angle:number): void{

    const canvas = this.myCanvas.nativeElement;
    const context = canvas.getContext('2d');
    if (!context)
    {
       return;
    }

    // context.save();
    //  context.translate(point.xPx, point.yPx);
    //context.rotate(angle);
console.log('Drawing plane at:', point.xPx, point.yPx, 'with angle:', angle);

    context.drawImage(this.plane, point.xPx - 10, point.yPx - 10, 5, 5);
    // context.restore();
  }

  private getAngleBetweenPoints(p1: Point, p2: Point): number {
    const deltaY = p2.yPx - p1.yPx;
    const deltaX = p2.xPx - p1.xPx;
    return Math.atan2(deltaY, deltaX) + Math.PI / 2;
  }
}
