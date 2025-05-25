import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { AnimationComponent } from './animation/animation.component';
import { RoutePlanningComponent } from './route-planning/route-planning.component';

const routes: Routes = [
  //{ path: '', redirectTo: '/home', pathMatch: 'full' },
    {
     path: '',
     component: HomeComponent 
    },
    {
     path: 'animation',
      component: AnimationComponent
    },
    {
      path:'routePlanning',
      component: RoutePlanningComponent
    }

  // { path: 'contact', component: ContactComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
