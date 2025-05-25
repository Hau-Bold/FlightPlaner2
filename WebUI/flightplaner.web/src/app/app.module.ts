import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { provideHttpClient } from '@angular/common/http';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterOutlet } from '@angular/router';
import { AsyncPipe } from '@angular/common';
import { AnimationComponent } from './animation/animation.component';
import { HomeComponent } from './home/home.component';
import { RoutePlanningComponent } from './route-planning/route-planning.component';

@NgModule({
  declarations: [
    AppComponent,
    AnimationComponent,
    HomeComponent,
    RoutePlanningComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    RouterOutlet,
    AsyncPipe,
    FormsModule,
    ReactiveFormsModule
  ],
  providers: [provideHttpClient()],
  bootstrap: [AppComponent]
})
export class AppModule { }

//RouterOutlet,
//AsyncPipe,
//FormsModule,
//ReactiveFormsModule
