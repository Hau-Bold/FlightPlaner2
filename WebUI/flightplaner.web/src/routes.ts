// src/app/routes.ts
import { Routes } from '@angular/router';
import { HomeComponent } from './app/home/home.component';
import { RoutePlanningComponent } from './app/route-planning/route-planning.component';
import { AnimationComponent } from './app/animation/animation.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'plan', component: RoutePlanningComponent },
  { path: 'animation', component: AnimationComponent } 
];
