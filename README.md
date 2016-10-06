# LeaseLock

This is a proof of concept where multiple instance can be run but only one can obtain 'the lease token'.

It requires atomic storage that supports optimistic concurrency control.


# Why

This can be used for simple master election in multi instance apps. Its not transactional but it the tasks executed are short enough and the lease duration long enough that specific instance should be the only instance performancing a specific task for that resource identifier.


# Process

1. Fetch the shared data
2. If it does not exist, create it and mention that this process it the 'owner'
3. It should now exist.
4. Verify if the token is expired, if so, update the token.
5. Check if the lease token is owned by the process discriminator identifier and if yes, update the token lease duration.

If anything of the above fails then we didn't acquire the lease token.


# Rules of engagement

1. All participants need to play by the rules.
2. The lease duration *must* be corrected for the maximum allowed clock drift. If clocks are in sync.
3. Graceful shutdown should result in other instance to obtain the lease token as the token is released during dispose.
4. If the instance crashes the lease token will not be updated meaning participants wait until the lease token is expired.
5. If the instance restarts and uses the same discriminator it will reuse its lease token.




# Sample

The sample can be invoked more than once. You should see that only one instance obtains the lease token the others obey :-)
