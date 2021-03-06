import {ChangeDetectorRef, Component, OnDestroy} from '@angular/core';
import {MediaMatcher} from "@angular/cdk/layout";
import {ReaderStatusService} from "./service/reader-status.service";
import {CheckpointService} from "./service/checkpoint.service";
import {OptionsService} from "./service/options.service";

@Component({
  selector: 'app-root',
  template: `      
    <mat-toolbar color="primary" class="fixed-top navbar navbar-dark">
        <span class="d-flex flex-shrink-0">
            <a class="navbar-brand" routerLink="">Checkpoint Service</a>
        </span>
        <div class="flex-grow-1">
          <mat-chip-list [hidden]="!readerStatusService.active">
            <mat-chip color="accent" selected>
                <i class="material-icons {{readerStatusService.statusIconClass}}">{{readerStatusService.statusIcon}}</i>
                {{checkpointService.checkpointsCount}}
            </mat-chip>
          </mat-chip-list>
        </div>
        <mat-divider></mat-divider>        
        <button mat-icon-button (click)="sidenav.toggle()"
                [hidden]="!mobileQuery.matches"><i class="material-icons">menu</i></button>
    </mat-toolbar>
    <mat-sidenav-container class="h-100">
        <mat-sidenav #sidenav [mode]="mobileQuery.matches ? 'over' : 'side'" [opened]="!mobileQuery.matches" position="end" 
                     [fixedInViewport]="mobileQuery.matches"
                        [fixedTopGap]="mobileQuery.matches ? 56 : 64"
                     (click)="mobileQuery.matches ? sidenav.toggle() : false">
            <mat-nav-list>
                <a mat-list-item routerLinkActive="text-danger" routerLink="/dash">Dashboard</a>
                <a mat-list-item routerLinkActive="text-danger" routerLink="/low-rps">Low RPS</a>
                <a mat-list-item routerLinkActive="text-danger" routerLink="/monitor">Checkpoints</a>
                <a mat-list-item routerLinkActive="text-danger" routerLink="/options">Options</a>
                <a mat-list-item routerLinkActive="text-danger" routerLink="/tags">Tags</a>
                <a mat-list-item routerLinkActive="text-danger" routerLink="/logs">Logs</a>
                <a mat-list-item routerLinkActive="text-danger" routerLink="/cleanup">Cleanup</a>
                <mat-divider></mat-divider>
                <a mat-list-item href="/files" (click)="goto('/files')">File Browser</a>
                <a mat-list-item (click)="portainer('9000')">Portainer</a>
                <mat-divider></mat-divider>
                <a mat-list-item disabled>{{optionsService.version}}</a>                                
            </mat-nav-list>
        </mat-sidenav>

        <mat-sidenav-content>            
            <div class="container-fluid d-flex flex-column h-100">
                <router-outlet></router-outlet>
            </div>
        </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: []
})
export class AppComponent implements OnDestroy {
  public mobileQuery: MediaQueryList;
  private readonly _mobileQueryListener: () => void;

  constructor(changeDetectorRef: ChangeDetectorRef, media: MediaMatcher,
              public readerStatusService: ReaderStatusService,
              public checkpointService: CheckpointService,
              public optionsService: OptionsService) {
    this.mobileQuery = media.matchMedia('(max-width: 600px)');
    this.mobileQuery.addListener(this._mobileQueryListener);
  }

  ngOnDestroy(): void {
    this.mobileQuery.removeListener(this._mobileQueryListener);
  }

  goto(s: string) {
    location.href=s;
  }

  portainer(port: string) {
    location.href = `${location.protocol}//${location.hostname}:${port}`;
  }
}
