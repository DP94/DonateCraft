package com.vypersw;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.vypersw.network.HttpHelper;
import com.vypersw.response.Revival;
import org.bukkit.*;
import org.bukkit.entity.EntityType;
import org.bukkit.entity.Player;
import org.bukkit.potion.PotionEffect;
import org.bukkit.potion.PotionEffectType;

import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.util.HashSet;
import java.util.Set;
import java.util.UUID;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;

public class ReanimationProtocol implements Runnable {

    private final Server server;
    private BlockingQueue<UUID> toRevive = new LinkedBlockingQueue<>();
    private final MessageHelper messageHelper;
    private final HttpHelper httpHelper;

    public ReanimationProtocol(Server server, MessageHelper messageHelper, HttpHelper httpHelper) {
        this.server = server;
        this.messageHelper = messageHelper;
        this.httpHelper = httpHelper;
    }

    @Override
    public void run() {
        reanimateEligiblePlayers();
    }

    private void reanimateEligiblePlayers() {
        HttpRequest request = httpHelper.buildGETHttpRequest("/unlocked");
        HttpClient client = HttpClient.newBuilder().build();
        client.sendAsync(request, HttpResponse.BodyHandlers.ofString()).thenAccept(asyncResponse -> {
            ObjectMapper objectMapper = new ObjectMapper();
            try {
                RevivalResponse response = objectMapper.readerFor(RevivalResponse.class).readValue(asyncResponse.body());
                if (response.getRevivals() != null && !response.getRevivals().isEmpty()) {
                    for (Revival revival : response.getRevivals()) {
                        UUID uuid = UUID.fromString(revival.getKey());
                        Player player = server.getPlayer(uuid);
                        if (player != null && player.getGameMode() == GameMode.SPECTATOR) {
                            server.broadcastMessage(messageHelper.getDonationMessageFromRevival(player, revival));
                            toRevive.offer(uuid);
                        }
                    }
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
        });

        Set<UUID> uuids = new HashSet<>();
        toRevive.drainTo(uuids);
        for (UUID uuid : uuids) {
            reanimatePlayer(uuid);
        }
    }

    public void reanimatePlayer(UUID uuid) {
        Player player = server.getPlayer(uuid);
        Revival revival = new Revival();
        revival.setKey(uuid.toString());
        //Extra checks just in case Minecraft has pinged the server again before our async call has come back
        if (player != null && player.isOnline() && (player.isDead() || player.getGameMode() == GameMode.SPECTATOR)) {
            server.getLogger().info("Attempting to revive " + player.getName());
            World currentPlayerWorld = player.getWorld();
            if (player.getBedSpawnLocation() == null) {
                player.teleport(currentPlayerWorld.getSpawnLocation());
            } else {
                player.teleport(player.getBedSpawnLocation());
            }
            player.setGameMode(GameMode.SURVIVAL);
            currentPlayerWorld.strikeLightningEffect(player.getLocation());
            currentPlayerWorld.playEffect(player.getLocation(), Effect.DRAGON_BREATH, 0);
            currentPlayerWorld.spawnEntity(player.getLocation(), EntityType.FIREWORK);
            addRespawnPotionEffects(player);
            server.broadcastMessage(ChatColor.GOLD + player.getName() + " " + ChatColor.GREEN + "has been revived!");
            httpHelper.fireAsyncPostRequestToServer("/revived", revival);
        } else if (player != null && player.isOnline() && !player.isDead()) {
            httpHelper.fireAsyncPostRequestToServer("/revived", revival);
        }
    }

    public void addRespawnPotionEffects(Player player) {
        player.addPotionEffect(new PotionEffect(PotionEffectType.GLOWING, 200, 100, true, true));
        player.addPotionEffect(new PotionEffect(PotionEffectType.REGENERATION, 200, 10, true, true));
        player.addPotionEffect(new PotionEffect(PotionEffectType.HEAL, 200, 10, true, true));
        player.addPotionEffect(new PotionEffect(PotionEffectType.DAMAGE_RESISTANCE, 200, 10, true, true));
    }
}
