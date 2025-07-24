import { AfterViewInit, Component, ElementRef, inject, ViewChild } from '@angular/core';
import { TargetAnimationState } from '../../Modules/targetAnimationState.model';
import { Point } from '../../Modules/point.model';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GPS } from '../../Modules/gps.model';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

@Component({
  standalone: true,
  selector: 'app-animation',
  templateUrl: './animation.component.html',
  styleUrls: ['./animation.component.css'],
  imports: [CommonModule, ReactiveFormsModule, RouterModule, HttpClientModule],
})
export class AnimationComponent implements AfterViewInit {
  private targetStates: { point: Point; state: TargetAnimationState }[] = [];
  private millerCoordinates: Point[] = [];
  private backgroundImageUrl = '../../assets/M1.jpg';
  private backgroundImageUrl2 = '../../assets/M2.jpg';
  private toggleBackground: boolean = true;
  public showRoute: boolean = false;
  public routeClickLabel: string = 'Route';

  http = inject(HttpClient);

  radius = 10;
  offset = 2;
  private animationId: number | null = null;
  private offscreenCanvas: HTMLCanvasElement;
  private offscreenContext: CanvasRenderingContext2D | null;
  public isAnimating: boolean = false;
  public imgWidth: number = 0;
  public imgHeight: number = 0;

  @ViewChild('canvas') myCanvas!: ElementRef;
  background: HTMLImageElement = new Image();

  constructor() {
    this.offscreenCanvas = document.createElement('canvas');
    this.offscreenContext = this.offscreenCanvas.getContext('2d');
  }

  ngAfterViewInit(): void {
    this.processImage();
  }

  public storeGuess(event: MouseEvent) {
    const canvas = this.myCanvas.nativeElement;
    const x = event.offsetX;
    const y = event.offsetY;
    const lon = (x / canvas.width) * 360 - 180;
    const lat = 90 - (y / canvas.height) * 180;
    alert(`Canvas coords: x=${x}, y=${y} : Approx GPS: lat=${lat}, lon=${lon}`);
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

          this.GetMillerCoordinates(canvas.width, canvas.height, this.background.width, this.background.height)
            .subscribe(points => {
              this.millerCoordinates = points;
              this.drawNeedles(this.millerCoordinates);
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
      this.GetMillerCoordinates(actualWidth, actualHeight, this.background.width, this.background.height)
        .subscribe(millerCoordinates => {
          this.targetStates = millerCoordinates.map(point => ({
            point,
            state: { counter: 0, radius: 10 }
          }));

          if (this.offscreenCanvas && context) {
            context.clearRect(0, 0, canvas.width, canvas.height);
            context.drawImage(this.offscreenCanvas, 0, 0);
          }

          this.animateAllTargets();
        });
    } else {
      this.stopAnimation();
    }
  }

  private animateAllTargets(): void {
    const canvas = this.myCanvas.nativeElement;
    const context = canvas.getContext('2d');
    if (!context) return;

    let lastRenderTime = 0;
    const frameInterval = 150;

    const animateFrame = (timestamp: number) => {
      if (!this.isAnimating) return;

      const elapsed = timestamp - lastRenderTime;
      if (elapsed >= frameInterval) {
        lastRenderTime = timestamp;

        context.clearRect(0, 0, canvas.width, canvas.height);
        context.drawImage(this.offscreenCanvas, 0, 0);

        
        if (this.showRoute) {
          this.drawRoute(context, this.millerCoordinates);
        }

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
          this.drawRoute(context, this.millerCoordinates);
        }

        this.drawNeedles(this.millerCoordinates);
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
          this.drawRoute(context, this.millerCoordinates);
        }

        this.drawNeedles(this.millerCoordinates);
      }
    }
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
    if (!context) return;

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

  private GetMillerCoordinates(actualWidth: number, actualHeight: number, imgWidth: number, imgHeight: number): Observable<Point[]> {
    const url = `https://localhost:7182/api/GPS/GetMillerCoordinates?actualWidth=${actualWidth}&actualHeight=${actualHeight}&imgWidth=${imgWidth}&imgHeight=${imgHeight}`;
    return this.http.get<Point[]>(url);
  }

  private GetCoordinates(): Observable<GPS[]> {
    return this.http.get<GPS[]>('https://localhost:7182/api/GPS');
  }
}
