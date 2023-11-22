# Supporting procfs in dotnet-monitor/diagnostics

## Scenarios

We want to be able to read diagnostic information from a dotnet process on demand, without reconfiguring or restarting the app.
In k8, this can be accomplished in one of two ways:

### Kubectl debug


### High privilage daemonset

This is similiar to the configuration of Azure Profiler, where a daemonset host volume mounts /proc. This allows a single high privilage
container to read the diagnostic information from all pods on the node.

### kubectl debug


## Examples

Typical ipc

```
/tmp/dotnet-diagnostic-<pid>-35261003-socket
```

Procfs

DaemonSet view (host mapped /proc)

```
/proc/<hostpid>/root/tmp/dotnet-diagnostic-<pid>-35261003-socket
```

kubectl debug view

```
/proc/<pid>/root/tmp/dotnet-diagnostic-<pid>-35261003-socket
```
### Other considerations

