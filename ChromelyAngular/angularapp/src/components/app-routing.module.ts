import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
      path: '',
      children: [
        { path: 'home', loadChildren: './home/home.module#HomeModule'},
        { path: 'login', loadChildren: './login/login.module#LoginModule'},
        { path: 'persons', loadChildren: './persons/persons.module#PersonsModule' },
        { path: 'tmdb', loadChildren: './tmdb/tmdb.module#TmdbModule'},
        { path: 'todolist', loadChildren: './todolist/todolist.module#TodoListModule'},
        { path: 'messagerouter', loadChildren: './messagerouter/messagerouter.module#MessageRouterModule'},
        { path: 'ajaxxhr', loadChildren: './ajaxxhr/ajaxxhr.module#AjaxXhrModule'},
        { path: 'javascriptdemo', loadChildren: './javascriptdemo/javascriptdemo.module#JavaScriptDemoModule'},
        { path: 'html5tests', loadChildren: './others/html5tests.module#Html5TestsModule'},
        { path: 'html6tests', loadChildren: './others/html6tests.module#Html6TestsModule'},
        { path: 'shakatests', loadChildren: './others/googleshakatests.module#GoogleShakaTestsModule'}
      ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
