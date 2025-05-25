import { AfterViewInit, Component, ElementRef, ViewChild, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Point } from '../../Modules/point.model';
import { Observable } from 'rxjs';
import { GPS } from '../../Modules/gps.model';
import { TargetAnimationState } from '../../Modules/targetAnimationState.model';

@Component({
  selector: 'app-animation',
  templateUrl: './animation.component.html',
  styleUrls: ['./animation.component.css']
})
export class AnimationComponent implements AfterViewInit {

  private targetStates: { point: Point; state: TargetAnimationState }[] = [];
  private millerCoordinates: Point[]=[];

  http = inject(HttpClient);

  radius = 10;
  offset = 2; // Adjust for growth speed
  counter = 0;
  private animationId: number | null = null;
  private offscreenCanvas: HTMLCanvasElement;
  private offscreenContext: CanvasRenderingContext2D | null;
  public isAnimating: boolean = false; // Boolean to control animation state
  public imgWidth: number = 0;
  public imgHeight: number = 0;

  @ViewChild('canvas') myCanvas!: ElementRef;

  constructor() {
    // Create an offscreen canvas for the background
    this.offscreenCanvas = document.createElement('canvas');
    this.offscreenContext = this.offscreenCanvas.getContext('2d');
  }

  ngAfterViewInit(): void {
    this.processImage();
  }

  public storeGuess(event: MouseEvent) {
    const canvas = this.myCanvas.nativeElement;
    const x = event.offsetX; // Get x coordinate
    const y = event.offsetY; // Get y coordinate

    // Convert pixel coordinates to longitude
    const lon = (x / canvas.width) * 360 - 180;

    // Convert pixel coordinates to latitude using the revised formula
    const lat = 90 - (y / canvas.height) * 180; // Revised calculation

    alert(`Canvas coords: x=${x}, y=${y} : Approx GPS: lat=${lat}, lon=${lon}  `);
  }

  processImage() {
    const canvas: HTMLCanvasElement = this.myCanvas.nativeElement;
    this.fitToContainer(canvas);

    // Set the size of the offscreen canvas
    this.offscreenCanvas.width = canvas.width;
    this.offscreenCanvas.height = canvas.height;

    const background = new Image();
    background.src = "../../assets/Mercator-projection.jpg";
    background.onload = () => {
      if (this.offscreenContext) {
        // Draw the background onto the offscreen canvas
        this.offscreenContext.drawImage(background, 0, 0, canvas.width, canvas.height);
        // Draw the background immediately on the main canvas
        const context = canvas.getContext('2d');
        if (context) {
          context.drawImage(this.offscreenCanvas, 0, 0);

          this.imgWidth = this.offscreenCanvas.width;
          this.imgHeight = this.offscreenCanvas.height;

          // Fetch and draw the initial points (needles)
          this.GetMillerCoordinates(canvas.width, canvas.height)
            .subscribe(points => {
              this.millerCoordinates=points;
              this.drawNeedles(this.millerCoordinates);   // Draw needles on the canvas
            });
        }
      }
    };
  }

  fitToContainer(canvas: HTMLCanvasElement) {
    // Make it visually fill the positioned parent
    canvas.style.width = '100%';
    canvas.style.height = '100%';
    // ...then set the internal size to match
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
      // SUBSCRIBE to get the coordinates
      this.GetMillerCoordinates(actualWidth, actualHeight).subscribe(millerCoordinates => {
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
  const frameInterval = 150; // milliseconds between frames (controls pulse speed)

  const animateFrame = (timestamp: number) => {
    if (!this.isAnimating) return;

    const elapsed = timestamp - lastRenderTime;
    if (elapsed >= frameInterval) {
      lastRenderTime = timestamp;

      // Clear and redraw background
      context.clearRect(0, 0, canvas.width, canvas.height);
      context.drawImage(this.offscreenCanvas, 0, 0);

      // Draw pulsing circles
      this.targetStates.forEach(({ point, state }) => {
        context.beginPath();
        context.arc(point.xPx, point.yPx, state.radius, 0, 2 * Math.PI);
        context.strokeStyle = 'red';
        context.stroke();

        context.font = '12px Arial';
        context.fillStyle = 'black';
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

  // Defer drawing to the next frame, after any queued animation frame finishes
  requestAnimationFrame(() => {
    const canvas = this.myCanvas.nativeElement;
    const context = canvas.getContext('2d');
    if (context) {
      context.clearRect(0, 0, canvas.width, canvas.height);
      context.drawImage(this.offscreenCanvas, 0, 0);
      this.drawNeedles(this.millerCoordinates); // safe to draw here
    }
  });
}


  private drawNeedles(points: Point[]): void {
    const canvas = this.myCanvas.nativeElement;
    const context = canvas.getContext('2d');
    if (!context) return;

    points.forEach(point => {
      // Draw a small "needle" (e.g., a triangle or a red dot)
      context.beginPath();
      context.moveTo(point.xPx, point.yPx);
      context.lineTo(point.xPx - 4, point.yPx + 10);
      context.lineTo(point.xPx + 4, point.yPx + 10);
      context.closePath();
      context.fillStyle = 'red';
      context.fill();

      // Draw the city name above the needle
      context.font = '12px Arial';
      context.fillStyle = 'black';
      context.textAlign = 'center';
      context.fillText(point.city, point.xPx, point.yPx - 8);
    });
  }

  private GetMillerCoordinates(actualWidth: number, actualHeight: number): Observable<Point[]> {
    const url = `https://localhost:7182/api/GPS/GetMillerCoordinates?actualWidth=${actualWidth}&actualHeight=${actualHeight}`;
    return this.http.get<Point[]>(url);
  }

  private GetCoordinates(): Observable<GPS[]> {
    return this.http.get<GPS[]>('https://localhost:7182/api/GPS');
  }
}
